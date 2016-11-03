namespace Bad_Broker_Test_Task
{
    using System ;
    using System.Collections.Generic ;
    using System.Globalization ;
    using System.Linq ;
    using System.Net ;
    using System.Runtime.Serialization ;
    using System.ServiceModel ;
    using System.ServiceModel.Activation ;
    using JetBrains.Annotations ;
    using Newtonsoft.Json ;
    using Newtonsoft.Json.Linq ;
    using Properties ;

    /// <summary>
    /// </summary>
    [ ServiceContract ( Namespace = "Bad_Broker_Test_Task" ) ]
    [ AspNetCompatibilityRequirements (
        RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed ) ]
    public class CurrencyExchangeService
    {
        /// <summary>
        /// </summary>
        [ OperationContract ]
        public string DoWork
            (
            DateTime a ,
            DateTime b ,
            double n )
        {
            var handler = new DealHandler ( ) ;

            var analysisData = handler.GetDealWithGreatestProfit
                (
                    a ,
                    b ,
                    n
                ) ;

            var greatestProfit = analysisData.GreatestProfit ;

            var resultAsString = handler.AnalysisResultToString
                (
                    greatestProfit ) ;

            return resultAsString ;
        }

        /// <summary>
        /// </summary>
        [OperationContract]
        public CurrencyDealAnalisysResult DoWork2
            (
            DateTime a,
            DateTime b,
            double n
            )
        {
            var handler = new DealHandler();

            var analysisData = handler.GetDealWithGreatestProfit
                (
                    a ,
                    b ,
                    n 
                ) ;

            return analysisData ;

        }
    }

    [DataContract]
    public class CurrencyDealAnalisysResult
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        [DataMember]
        public List<ExchangeRates> ExchangeRatesList { private get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        [DataMember]
        public AnalysisResult GreatestProfit { get; set; }

        /// <summary>
        /// Currencies for exchange
        /// </summary>
        [DataMember]
        public string[] Currencies { get; set; }
    }

    /// <summary>
    /// Routines methods
    /// </summary>
    public static class Routines
    {
        /// <summary>
        /// split string by <paramref name="separators"/> in array
        /// </summary>
        /// <param name="splitsString">string to split </param>
        /// <param name="separators">separators chars</param>
        /// <returns>strings</returns>
        [ NotNull ]
        public static IEnumerable < string > StringToArray
            (
            [ CanBeNull ] string splitsString ,
            [ CanBeNull ] char [ ] separators )
        {
            string [ ] stringsArray =
            {
            } ;
            if ( !string.IsNullOrEmpty
                      (
                          splitsString ) )
            {

                stringsArray = splitsString.Split
                    (
                        separators ,
                        StringSplitOptions.RemoveEmptyEntries ) ;
            }
            var length = stringsArray.LongLength ;
            if ( length > 0 )
            {
                var begin = stringsArray.GetLowerBound
                    (
                        0 ) ;
                var end = stringsArray.GetUpperBound
                    (
                        0 ) ;
                for ( var index = begin ;
                      index <= end ;
                      index++ )
                {
                    var element = stringsArray [ index ] ;
                    if ( element != null )
                    {
                        stringsArray [ index ] = element.Trim ( ) ;
                    }
                }
            }
            return stringsArray ;
        }
    }

    /// <summary>
    /// Currency Exchange Rates
    /// </summary>
    [DataContract]
    public class ExchangeRates
    {

        /// <summary>
        /// <see langword="base"/> currency of rates
        /// </summary>
        [DataMember]
        private string Base { get ; set ; }

        /// <summary>
        /// Date of exchange rates
        /// </summary>
        [DataMember]
        public DateTime RatesDate { get ; private set ; }

        /// <summary>
        /// Rates of exchanges date
        /// </summary>
        [DataMember]
        private Dictionary < string , double > DatesRates { get ; set ; }

        /// <summary>
        /// Exchange rates from Fixer rates
        /// </summary>
        public ExchangeRates ( [ CanBeNull ] FixerHandler.FixerExchangeRates fixerExchangeRates )
        {
            if ( fixerExchangeRates != null )
            {
                this.Base = fixerExchangeRates.Base ;
                this.DatesRates = fixerExchangeRates.DatesRates ;
                this.RatesDate = FixerHandler.ConvertFixerDateStringToDate
                    (
                        fixerExchangeRates.Date ) ;
            }
        }

        /// <summary>
        /// Get one rate by currency
        /// </summary>
        /// <param name="source"><paramref name="source"/> of exchange</param>
        /// <param name="target">target of exchange</param>
        /// <returns>one rate</returns>
        public double GetRate
            (
            [ CanBeNull ] string source ,
            [ CanBeNull ] string target )
        {
            double targetRate = 0 ;


            if ( ( this.DatesRates != null )
                 && ( this.Base == source ) )
            {
                foreach ( var rate in this.DatesRates )
                {
                    if ( rate.Key == target )
                    {
                        targetRate = rate.Value ;
                        break ;
                    }
                }
            }

            return targetRate ;
        }
    }

    /// <summary>
    /// <see cref="Default"/> for http://fixer.io service
    /// </summary>
    public static class FixerHandler
    {
        /// <summary>
        /// Base currency of exchange operations
        /// </summary>
        private static string BaseCurrency { get ; set ; }

        /// <summary>
        /// Base currency of exchange operations
        /// </summary>
        private const string C_BaseCurrency = "USD" ;

        /// <summary>
        /// Currencies which possible to exchange
        /// </summary>
        private static string TradingCurrencies { get ; set ; }

        /// <summary>
        /// Currencies which possible to exchange
        /// </summary>
        private const string C_TradingCurrencies = "RUB,EUR,GBP,JPY" ;

        /// <summary>
        /// Spelling currencies exchange rates for Fixer service
        /// </summary>
        private static string TradingCurrenciesParameterSpelling { get ; set ; }

        /// <summary>
        /// Spelling currencies exchange rates for Fixer service
        /// </summary>
        private const string C_TradingCurrenciesParameterSpelling = "symbols={0}" ;

        /// <summary>
        /// Spelling base currency for Fixer service
        /// </summary>
        private static string BaseCurrencyParameterSpelling { get ; set ; }

        /// <summary>
        /// Spelling base currency for Fixer service
        /// </summary>
        private const string C_BaseCurrencyParameterSpelling = "base={0}" ;

        /// <summary>
        /// Fixer service address
        /// </summary>
        private static string ExchangeRatesDataSource { get ; set ; }

        /// <summary>
        /// Fixer service address
        /// </summary>
        // ReSharper disable StringLiteralsWordIsNotInDictionary
        // ReSharper disable StringLiteralsWordIsNotInDictionary
        private const string C_ExchangesRatesDataSource = "HTTP://api.fixer.io/" ;

        // ReSharper restore StringLiteralsWordIsNotInDictionary
        // ReSharper restore StringLiteralsWordIsNotInDictionary

        /// <summary>
        /// Pattern of Fixer service request
        /// </summary>
        private static string ExchangeRatesRequestPattern { get ; set ; }

        /// <summary>
        /// Pattern of Fixer service request
        /// </summary>
        private const string C_ExchangesRatesRequestPattern = "{0}{1}?{2}&{3}" ;

        /// <summary>
        /// Step for run dates period in days units
        /// </summary>
        private static double RequestRatesDaysStep { get ; set ; }

        /// <summary>
        /// Step for run dates period in days units
        /// </summary>
        private const double C_RequestRatesDaysStep = 1 ;

        /// <summary>
        /// Number of Fixer-date components
        /// </summary>
        private const int C_DateComponentsCount = 3 ;

        /// <summary>
        /// Index of Year component at Fixer-date
        /// </summary>
        private const int C_YearComponentIndex = 0 ;

        /// <summary>
        /// Index of Month component at Fixer-date
        /// </summary>
        private const int C_MonthComponentIndex = 1 ;

        /// <summary>
        /// Index of Day component at Fixer-date
        /// </summary>
        private const int C_DayComponentIndex = 2 ;

        /// <summary>
        /// Delimiter of dates components 
        /// </summary>
        private const string C_DatesComponentsDelimiter = "-" ;

        /// <summary>
        /// Char for padding date components
        /// </summary>
        private const char C_DatePaddingChar = '0' ;

        /// <summary>
        /// Width of Year component at Fixer-date
        /// </summary>
        private const int C_YearTotalWidth = 4 ;

        /// <summary>
        /// Width of Month component at Fixer-date
        /// </summary>
        private const int C_MonthTotalWidth = 2 ;

        /// <summary>
        /// Width of Day component at Fixer-date
        /// </summary>
        private const int C_DayTotalWidth = 2 ;

        /// <summary>
        /// Convert <see cref="T:System.DateTime"/> to Fixer-date
        /// </summary>
        /// <param name="date">Datetime to convert</param>
        /// <returns>string of Fixer-date</returns>
        private static string ConvertDateToFixerDateFormat
            (
            DateTime date )
        {
            var year = date.Year.ToString ( ).
                            PadLeft
                (
                    FixerHandler.C_YearTotalWidth ,
                    FixerHandler.C_DatePaddingChar ) ;
            var month = date.Month.ToString ( ).
                             PadLeft
                (
                    FixerHandler.C_MonthTotalWidth ,
                    FixerHandler.C_DatePaddingChar ) ;
            var day = date.Day.ToString ( ).
                           PadLeft
                (
                    FixerHandler.C_DayTotalWidth ,
                    FixerHandler.C_DatePaddingChar ) ;

            var fixerDate =
                string.Format
                    (
                        "{0}{1}{2}{1}{3}" ,
                        year ,
                        FixerHandler.C_DatesComponentsDelimiter ,
                        month ,
                        day
                    ) ;
            return fixerDate ;
        }

        /// <summary>
        /// Convert Fixer-date to <see cref="T:System.DateTime"/>
        /// </summary>
        /// <param name="fixerDateString">Fixer-date</param>
        /// <returns>DateTime</returns>
        public static DateTime ConvertFixerDateStringToDate
            (
            [ CanBeNull ] string fixerDateString )
        {
            var date = DateTime.MinValue ;

            string [ ] datesComponentsArray =
            {
            } ;

            if ( !string.IsNullOrEmpty
                      (
                          fixerDateString ) )
            {
                datesComponentsArray = fixerDateString
                    .Split
                    (
                        Convert.ToChar
                            (
                                FixerHandler.C_DatesComponentsDelimiter ) ) ;
            }

            var componentsCount = datesComponentsArray.LongLength ;

            if ( componentsCount >= FixerHandler.C_DateComponentsCount )
            {
                int year ;
                int month ;
                int day ;

                int.TryParse
                    (
                        datesComponentsArray [ FixerHandler.C_YearComponentIndex ] ,
                        out year ) ;


                int.TryParse
                    (
                        datesComponentsArray [ FixerHandler.C_MonthComponentIndex ] ,
                        out month ) ;

                int.TryParse
                    (
                        datesComponentsArray [ FixerHandler.C_DayComponentIndex ] ,
                        out day ) ;

                try
                {
                    date = new DateTime
                        (
                        year ,
                        month ,
                        day ) ;
                }
                catch ( Exception )
                {
                    // ignored
                }
            }

            return date ;
        }

        /// <summary>
        /// Exchange rates of Fixer format
        /// </summary>
        public class FixerExchangeRates
        {
            /// <summary>
            /// Property name for <see langword="base"/> currency component of JSON responce
            /// </summary>
            private const string C_BasePropertyName = "base" ;

            /// <summary>
            /// Property name for date of exchange rates component of JSON responce
            /// </summary>
            private const string C_DatePropertyName = "date" ;

            /// <summary>
            /// Property name for rates component of JSON responce
            /// </summary>
            private const string C_RatesPropertyName = "rates" ;

            /// <summary>
            /// Base currency of exchange rates
            /// </summary>
            public string Base { get ; }

            /// <summary>
            /// Fixer-date of exchange rates
            /// </summary>
            public string Date { get ; }

            /// <summary>
            /// Currencies exchange rates at date
            /// </summary>
            public Dictionary < string , double > DatesRates { get ; }


            /// <summary>
            /// Empty Fixer rates
            /// </summary>
            public FixerExchangeRates ( )
            {
                this.Base = string.Empty ;
                this.Date = string.Empty ;
                this.DatesRates = new Dictionary < string , double > ( ) ;
            }

            /// <summary>
            /// Fixer Rates from JSON responce
            /// </summary>
            /// <param name="rawJson">rates component of JSON responce</param>
            public FixerExchangeRates ( string rawJson )
            {
                var jExchangeRates = JObject.Parse
                    (
                        rawJson ) ;
                if ( jExchangeRates != null )
                {

                    var jBase = jExchangeRates [ FixerExchangeRates.C_BasePropertyName ] ;

                    var jDate = jExchangeRates [ FixerExchangeRates.C_DatePropertyName ] ;

                    this.Base = ( string ) JToken.FromObject
                                               (
                                                   jBase ) ;
                    this.Date = ( string ) JToken.FromObject
                                               (
                                                   jDate ) ;

                    var jRates = jExchangeRates [ FixerExchangeRates.C_RatesPropertyName ] ;
                    if ( jRates != null )
                    {
                        var rawRates = jRates.ToString ( ) ;
                        this.DatesRates = JsonConvert.DeserializeObject
                            < Dictionary < string , double > >
                            (
                                rawRates ) ;
                    }
                    else
                    {
                        this.DatesRates = new Dictionary < string , double > ( ) ;
                    }
                }
            }
        }

        /// <summary>
        /// Get exchange rates from Fixer
        /// </summary>
        /// <param name="requestString">string of request</param>
        /// <returns>Fixer exchange rates </returns>
        private static FixerExchangeRates GetExchangeRates
            (
            string requestString )
        {
            FixerExchangeRates exchangeRates ;
            var responceString = string.Empty ;

            if ( !string.IsNullOrEmpty
                      (
                          requestString ) )
            {
                var webclient = new WebClient ( ) ;

                try
                {
                    responceString = webclient.DownloadString
                        (
                            requestString ) ;
                }
                catch ( Exception )
                {
                    responceString = string.Empty ;
                }
            }

            if ( string.IsNullOrEmpty
                (
                    responceString ) )
            {
                exchangeRates = new FixerExchangeRates ( ) ;
            }
            else
            {
                exchangeRates = new FixerExchangeRates
                    ( responceString ) ;
            }

            return exchangeRates ;
        }

        /// <summary>
        /// Get Fixer exchange rate of dates period
        /// </summary>
        /// <param name="beginsDate">first day of dates period </param>
        /// <param name="endsDate">last day of dates period</param>
        /// <returns>Fixer exchange rates</returns>
        [ NotNull ]
        public static IEnumerable < FixerExchangeRates > GetFixerExchangeRates
            (
            DateTime beginsDate ,
            DateTime endsDate
            )
        {

            var exchangeRatesList = new List < FixerExchangeRates > ( ) ;

            var setting = Settings.Default ;

            if ( setting == null )
            {
                FixerHandler.BaseCurrency = FixerHandler.C_BaseCurrency ;
                FixerHandler.TradingCurrencies = FixerHandler.C_TradingCurrencies ;
                FixerHandler.BaseCurrencyParameterSpelling = FixerHandler.C_BaseCurrencyParameterSpelling ;
                FixerHandler.TradingCurrenciesParameterSpelling = FixerHandler.C_TradingCurrenciesParameterSpelling ;
                FixerHandler.ExchangeRatesRequestPattern = FixerHandler.C_ExchangesRatesRequestPattern ;
                FixerHandler.RequestRatesDaysStep = FixerHandler.C_RequestRatesDaysStep ;
                FixerHandler.ExchangeRatesDataSource = FixerHandler.C_ExchangesRatesDataSource ;
            }
            else
            {
                FixerHandler.BaseCurrency = setting.BaseCurrency ;
                FixerHandler.TradingCurrencies = setting.TradingCurrencies ;
                FixerHandler.BaseCurrencyParameterSpelling = setting.BaseCurrencyParameterSpelling ;
                FixerHandler.TradingCurrenciesParameterSpelling = setting.TradingCurrenciesParameterSpelling ;
                FixerHandler.ExchangeRatesRequestPattern = setting.ExchangeRatesRequestPattern ;
                FixerHandler.RequestRatesDaysStep = setting.RequestRatesDaysStep ;
                FixerHandler.ExchangeRatesDataSource = setting.ExchangeRatesDataSource ;
            }

            var baseCurrencyParameterPattern = FixerHandler.BaseCurrencyParameterSpelling ;
            var tradingCurrenciesParameterPattern = FixerHandler.TradingCurrenciesParameterSpelling ;
            var ratesRequestsPattern = FixerHandler.ExchangeRatesRequestPattern ;
            var exchangesRatesDataSource = FixerHandler.ExchangeRatesDataSource ;

            var nextDateStep = FixerHandler.RequestRatesDaysStep ;

            var sourceCurrency = FixerHandler.BaseCurrency ;
            var targetCurrencies = FixerHandler.TradingCurrencies ;

            var ratesRequestsPatternByDate = string.Empty ;

            if (
                !string.IsNullOrEmpty
                     (
                         baseCurrencyParameterPattern )
                && !string.IsNullOrEmpty
                        (
                            tradingCurrenciesParameterPattern )
                && !string.IsNullOrEmpty
                        (
                            ratesRequestsPattern )
                && !string.IsNullOrEmpty
                        (
                            exchangesRatesDataSource )
                )
            {

                var requestParameterSource = string.Format
                    (
                        baseCurrencyParameterPattern ,
                        sourceCurrency ) ;
                var requestParameterTarget = string.Format
                    (
                        tradingCurrenciesParameterPattern ,
                        targetCurrencies ) ;

                ratesRequestsPatternByDate = string.Format
                    (
                        ratesRequestsPattern ,
                        exchangesRatesDataSource ,
                        "{0}" ,
                        requestParameterSource ,
                        requestParameterTarget ) ;
            }

            if ( !string.IsNullOrEmpty
                      (
                          ratesRequestsPatternByDate ) )
            {
                var currentDate = beginsDate ;

                var needToSwapDates = ( endsDate < currentDate ) ;
                if ( needToSwapDates )
                {
                    currentDate = endsDate ;
                    endsDate = beginsDate ;
                }

                var previousDate = string.Empty ;

                var continueGetRates = endsDate >= currentDate ;

                while ( continueGetRates && ( nextDateStep > 0 ) )
                {
                    var sourceDate = FixerHandler.ConvertDateToFixerDateFormat
                        (
                            currentDate ) ;

                    var ratesRequest = string.Format
                        (
                            ratesRequestsPatternByDate ,
                            sourceDate ) ;

                    var exchangeRates = FixerHandler.GetExchangeRates
                        (
                            ratesRequest ) ;

                    if ( exchangeRates != null )
                    {
                        var isSameDate = previousDate.Equals
                                             (
                                                 exchangeRates.Date ,
                                                 StringComparison.InvariantCultureIgnoreCase )
                                         && !string.IsNullOrEmpty
                                                 (
                                                     exchangeRates.Date ) ;
                        if ( !isSameDate )
                        {
                            exchangeRatesList.Add
                                (
                                    exchangeRates ) ;
                        }

                        previousDate = exchangeRates.Date ?? string.Empty ;
                    }

                    currentDate = currentDate.AddDays
                        (
                            nextDateStep ) ;
                    continueGetRates = endsDate >= currentDate ;
                }

            }

            return exchangeRatesList ;
        }
    }


    /// <summary>
    /// many Currencies many Exchanges
    /// </summary>
    public class CurrenciesExchanges
    {
        /// <summary>
        /// exchanges source currency
        /// </summary>
        private string SourceCurrency { get ; }

        /// <summary>
        /// one Currency many Exchanges
        /// </summary>
        private class CurrencyExchanges
        {
            /// <summary>
            /// one Currency one Exchange
            /// </summary>
            public class Exchange
            {
                /// <summary>
                /// Date of exchange
                /// </summary>
                public DateTime Date { get ; }

                /// <summary>
                /// cost of exchange
                /// </summary>
                public double Rate { get ; }

                /// <summary>
                /// Exchange by rates , source and target currencies
                /// </summary>
                /// <param name="rates">exchange <paramref name="rates"/></param>
                /// <param name="source">source currency for exchange</param>
                /// <param name="target">target currency for exchange</param>
                public Exchange
                    (
                    [ CanBeNull ] ExchangeRates rates ,
                    [ CanBeNull ] string source ,
                    [ CanBeNull ] string target
                    )
                {
                    var date = DateTime.MinValue ;
                    double rate = double.NaN ;

                    if ( rates != null )
                    {
                        date = rates.RatesDate ;
                        rate = rates.GetRate
                            (
                                source ,
                                target ) ;
                    }

                    this.Date = date ;
                    this.Rate = rate ;
                }

            }

            /// <summary>
            /// target currency of exchange
            /// </summary>
            private string TargetCurrency { get ; }

            /// <summary>
            /// collections of exchanges
            /// </summary>
            public List < Exchange > Items { get ; }

            /// <summary>
            /// Currency exchanges by rates , source and target currencies
            /// </summary>
            public CurrencyExchanges
                (
                [ CanBeNull ] List < ExchangeRates > rawExchangeRates ,
                [ CanBeNull ] string source ,
                [ CanBeNull ] string target
                )
            {
                this.TargetCurrency = target ;
                this.Items = new List < Exchange > ( ) ;

                if ( rawExchangeRates != null )
                {
                    foreach ( var exchangeRates in rawExchangeRates )
                    {
                        var exchange = new Exchange
                            (
                            exchangeRates ,
                            source ,
                            this.TargetCurrency
                            ) ;
                        var rate = exchange.Rate ;
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        var isValidExchange =
                            ( rate != 0 )
                            && !double.IsNaN
                                    (
                                        rate ) ;
                        // ReSharper restore CompareOfFloatsByEqualityOperator
                        if ( isValidExchange )
                        {
                            this.Items.Add
                                (
                                    exchange ) ;
                        }
                    }
                }
            }

            /// <summary>
            /// Get all possible deals for one currency
            /// </summary>
            /// <param name="source"><paramref name="source"/> currency</param>
            /// <returns>suggestions for deal</returns>
            [ NotNull ]
            public List < DealSuggestion > GetCurrencySuggestions
                (
                string source )
            {
                var currencySuggestions = new List < DealSuggestion > ( ) ;

                var currencyExchangesItems = this.Items ;
                if ( currencyExchangesItems != null )
                {
                    foreach ( var openExchange in currencyExchangesItems )
                    {
                        if ( openExchange != null )
                        {
                            foreach ( var closeExchange in currencyExchangesItems )
                            {
                                if ( closeExchange != null )
                                {
                                    var open = openExchange.Date ;
                                    var close = closeExchange.Date ;
                                    var isValidCombination = close > open ;
                                    if ( isValidCombination )
                                    {
                                        var suggestion = new DealSuggestion
                                                         {
                                                             Source = source ,
                                                             Open = openExchange.Date ,
                                                             ConvertRate = openExchange.Rate ,
                                                             Target = this.TargetCurrency ,
                                                             Close = closeExchange.Date ,
                                                             BackConvertsRate = closeExchange.Rate
                                                         } ;

                                        currencySuggestions.Add
                                            (
                                                suggestion ) ;
                                    }
                                }
                            }
                        }
                    }
                }
                return currencySuggestions ;
            }
        }

        /// <summary>
        /// Get all possible currency exchanges from given rates
        /// </summary>
        /// <param name="exchangeRates">exchange rates</param>
        /// <param name="currencies">collections of currencies </param>
        /// <param name="baseCurrency">base currency of rates </param>
        public CurrenciesExchanges
            (
            [ CanBeNull ] List < ExchangeRates > exchangeRates ,
            [ CanBeNull ] string [ ] currencies ,
            [ CanBeNull ] string baseCurrency )
        {
            this.Items = new List < CurrencyExchanges > ( ) ;
            this.SourceCurrency = baseCurrency ;

            if ( currencies != null )
            {
                foreach ( var currency in currencies )
                {
                    var currencyExchanges = new CurrencyExchanges
                        (
                        exchangeRates ,
                        this.SourceCurrency ,
                        currency
                        ) ;

                    var isValidCurrencyExchanges = false ;

                    if ( currencyExchanges.Items != null )
                    {
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        isValidCurrencyExchanges = currencyExchanges.Items.Count > 0 ;
                        // ReSharper restore CompareOfFloatsByEqualityOperator    
                    }

                    if ( isValidCurrencyExchanges )
                    {
                        this.Items.Add
                            (
                                currencyExchanges ) ;
                    }

                }
            }
        }

        /// <summary>
        /// collection of currency exchanges
        /// </summary>
        private List < CurrencyExchanges > Items { get ; }

        /// <summary>
        /// Get all possible exchanges for all currencies
        /// </summary>
        /// <returns>Suggestions for deals </returns>
        public List < DealSuggestion > GetDealsSuggestion ( )
        {
            var suggestions = new List < DealSuggestion > ( ) ;
            var source = this.SourceCurrency ;
            var currencyExchangeses = this.Items ;

            if ( !string.IsNullOrEmpty
                      (
                          source )
                 && ( currencyExchangeses != null ) )
            {
                foreach ( var currencyExchanges in currencyExchangeses )
                {
                    if ( currencyExchanges != null )
                    {
                        var currencySuggestions = currencyExchanges.GetCurrencySuggestions
                            (
                                source ) ;
                        suggestions.AddRange
                            (
                                currencySuggestions ) ;
                    }
                }
            }

            return suggestions ;
        }
    }

    /// <summary>
    /// Suggestion for deal
    /// </summary>
    public class DealSuggestion
    {
        /// <summary>
        /// source currency
        /// </summary>
        public string Source { get ; set ; }

        /// <summary>
        /// target currency
        /// </summary>
        public string Target { get ; set ; }

        /// <summary>
        /// open deal date
        /// </summary>
        public DateTime Open { get ; set ; }

        /// <summary>
        /// close deal date
        /// </summary>
        public DateTime Close { get ; set ; }

        /// <summary>
        /// rate for convert source currency to target currency
        /// </summary>
        public double ConvertRate { private get ; set ; }

        /// <summary>
        /// rate for back convert from target currency to source currency
        /// </summary>
        public double BackConvertsRate { private get ; set ; }

        /// <summary>
        /// Price of brokers service for one day
        /// </summary>
        private const double C_BrokerServicePrice = 1 ;

        /// <summary>
        /// Calculate profit of suggestion
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>profit of currency deal </returns>
        public double CalculateSuggestionsProfit
            (
            double amount )
        {
            var profit = double.NaN ;
            var closeDate = this.Close ;
            var openDate = this.Open ;

            var daysBetweenCloseAndOpen = Math.Truncate
                (
                    ( closeDate - openDate ).TotalDays ) ;

            if (
                ( daysBetweenCloseAndOpen > 0 )
                // ReSharper disable CompareOfFloatsByEqualityOperator
                && ( amount != 0 )
                // ReSharper restore CompareOfFloatsByEqualityOperator
                && !double.IsNaN
                        (
                            amount ) )
            {
                var brokerFee = DealSuggestion.C_BrokerServicePrice * daysBetweenCloseAndOpen ;

                profit = ( ( this.ConvertRate * amount ) / this.BackConvertsRate ) - brokerFee ;
            }

            return profit ;
        }
    }

    /// <summary>
    /// Collection of currencies deals
    /// </summary>
    public class CurrenciesDeals
    {
        /// <summary>
        /// Deal of currency exchange from base currency to any and exchange back
        /// </summary>
        private class CurrencyDeal
        {
            /// <summary>
            /// profit of deal
            /// </summary>
            public double Profit { get ; }

            /// <summary>
            /// open deal date
            /// </summary>
            public DateTime OpenDate { get ; }

            /// <summary>
            /// close deal date
            /// </summary>
            public DateTime CloseDate { get ; }

            /// <summary>
            /// source currency of deal
            /// </summary>
            public string SourceCurrency { get ; }

            /// <summary>
            /// amount of source currency of deal
            /// </summary>
            public double SourceAmount { get ; }

            /// <summary>
            /// target ( ancillary ) currency of deal
            /// </summary>
            public string TargetCurrency { get ; }

            /// <summary>
            /// </summary>
            /// <param name="deal"></param>
            /// <param name="amount"></param>
            public CurrencyDeal
                (
                [ CanBeNull ] DealSuggestion deal ,
                double amount )
            {
                var closeDate = DateTime.MinValue ;
                var openDate = DateTime.MinValue ;
                var sourceCurrency = string.Empty ;
                var targetCurrency = string.Empty ;
                var profit = double.NaN ;

                if ( deal != null )
                {
                    closeDate = deal.Close ;
                    openDate = deal.Open ;
                    sourceCurrency = deal.Source ;
                    targetCurrency = deal.Target ;
                    profit = deal.CalculateSuggestionsProfit
                        (
                            amount ) ;
                }

                this.CloseDate = closeDate ;
                this.OpenDate = openDate ;
                this.SourceCurrency = sourceCurrency ;
                this.TargetCurrency = targetCurrency ;
                this.SourceAmount = amount ;
                this.Profit = profit ;
            }
        }

        /// <summary>
        /// currencies deals
        /// </summary>
        private List < CurrencyDeal > Items { get ; }

        /// <summary>
        /// Currencies of deals
        /// </summary>
        private string [ ] Currencies { get ; }

        /// <summary>
        /// source currency of deals
        /// </summary>
        private string SourceCurrency { get ; }

        /// <summary>
        /// simple constructor
        /// </summary>
        public CurrenciesDeals
            (
            [ CanBeNull ] string [ ] currencies ,
            [ CanBeNull ] string source )
        {
            this.SourceCurrency = source ?? string.Empty ;
            this.Currencies = currencies ?? new string [ ]
                                            {
                                            } ;
            this.Items = new List < CurrencyDeal > ( ) ;
        }

        /// <summary>
        /// validate currency deal and add to collection 
        /// </summary>
        /// <param name="currencyDeal">deal to add</param>
        private void AddItem
            (
            CurrencyDeal currencyDeal )
        {
            var source = string.Empty ;
            var target = string.Empty ;
            string [ ] currencies = null ;

            if ( currencyDeal != null )
            {
                source = currencyDeal.SourceCurrency ;
                target = currencyDeal.TargetCurrency ;
                currencies = this.Currencies ;
            }

            var isSource = false ;
            if ( !string.IsNullOrEmpty
                      (
                          source ) )
            {
                isSource = source.Equals
                    (
                        this.SourceCurrency ,
                        StringComparison.InvariantCultureIgnoreCase )
                    ;
            }

            var containTarget = false ;

            if ( ( currencies != null )
                 && !string.IsNullOrEmpty
                         (
                             target ) )
            {
                foreach ( var currency in currencies )
                {
                    if ( currency != null )
                    {
                        containTarget = currency.Equals
                            (
                                target ,
                                StringComparison.InvariantCultureIgnoreCase ) ;
                    }
                    if ( containTarget )
                    {
                        break ;
                    }
                }
            }

            var mayAdd = isSource && containTarget ;

            var currencyDeals = this.Items ;
            if ( mayAdd )
            {
                currencyDeals?.Add
                    (
                        currencyDeal ) ;
            }
        }

        /// <summary>
        /// Assume deals suggestions
        /// </summary>
        /// <param name="dealSuggestions">exchange rates collection</param>
        /// <param name="amount"></param>
        public void AssumeSuggestions
            (
            IEnumerable < DealSuggestion > dealSuggestions ,
            double amount )
        {
            if ( dealSuggestions != null )
            {
                foreach ( var suggestion in dealSuggestions )
                {
                    var currencyDeal = new CurrencyDeal
                        (
                        suggestion ,
                        amount ) ;
                    var profit = currencyDeal.Profit ;
                    var isValidDeal =
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        ( profit != 0 )
                        // ReSharper restore CompareOfFloatsByEqualityOperator
                        && !double.IsNaN
                                (
                                    profit ) ;
                    if ( isValidDeal )
                    {
                        this.AddItem
                            (
                                currencyDeal ) ;
                    }

                }
            }
        }

        /// <summary>
        /// Get deal with greatest profit
        /// </summary>
        /// <returns>Currency deal</returns>
        public AnalysisResult GetDealWithGreatestProfit ( )
        {
            var currencyDealWithGreatestProfit = new CurrencyDeal
                (
                null ,
                double.NaN ) ;
            var items = this.Items ;
            if ( items != null )
            {
                foreach ( var currencyDeal in items )
                {
                    if ( currencyDeal != null )
                    {
                        {
                            var isBestProfit =
                                ( currencyDeal.Profit > currencyDealWithGreatestProfit.Profit )
                                || double.IsNaN
                                       (
                                           currencyDealWithGreatestProfit.Profit ) ;
                            if ( isBestProfit )
                            {
                                currencyDealWithGreatestProfit = currencyDeal ;
                            }
                        }
                    }
                }
            }

            var result = new AnalysisResult
                         {
                             Profit = currencyDealWithGreatestProfit.Profit ,
                             Close = currencyDealWithGreatestProfit.CloseDate ,
                             Open = currencyDealWithGreatestProfit.OpenDate ,
                             Source = currencyDealWithGreatestProfit.SourceCurrency ,
                             Target = currencyDealWithGreatestProfit.TargetCurrency ,
                             Amount = currencyDealWithGreatestProfit.SourceAmount
                         } ;

            return result ;

        }
    }

    /// <summary>
    /// Result of analysis period of currency exchange rates
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>date of exchange from source to target currency
        /// </summary>
        [DataMember]
        public DateTime Open { get ; set ; }

        /// <summary>
        /// date of exchange back from target to source currency
        /// </summary>
        [DataMember]
        public DateTime Close { get ; set ; }

        /// <summary>
        /// source currency
        /// </summary>
        [DataMember]
        public string Source { get ; set ; }

        /// <summary>
        /// target currency
        /// </summary>
        [DataMember]
        public string Target { get ; set ; }

        /// <summary>
        /// profit of exchanges
        /// </summary>
        [DataMember]
        public double Profit { get ; set ; }

        /// <summary>
        /// amount to exchanges
        /// </summary>
        [DataMember]
        public double Amount { get ; set ; }
    }

    /// <summary>
    /// </summary>
    public class DealHandler
    {
        /// <summary>
        /// </summary>
        public DealHandler ( )
        {

            var setting = Settings.Default ;

            if ( setting == null )
            {
                this.BaseCurrency = DealHandler.C_BaseCurrency ;
                this.TradingCurrencies = DealHandler.C_TradingCurrencies ;
                this.CurrenciesSeparators = DealHandler.C_CurrenciesSeparators ;
                this.ResultReportPattern = DealHandler.C_ResultReportPattern ;
            }
            else
            {
                this.BaseCurrency = setting.BaseCurrency ;
                this.TradingCurrencies = setting.TradingCurrencies ;
                this.CurrenciesSeparators = setting.CurrenciesSeparators ;
                this.ResultReportPattern = setting.ResultReportPattern ;
            }

            var currenciesString = this.TradingCurrencies ;

            var separators = this.CurrenciesSeparators ;
            if ( separators != null )
            {
                var currenciesSeparators = separators.ToCharArray ( ) ;
                var currencies = Routines.StringToArray
                    (
                        currenciesString ,
                        currenciesSeparators ) ;

                this.Currencies = currencies.ToArray ( ) ;
            }
            else
            {
                this.Currencies = new string [ ]
                                  {
                                  } ;
            }
        }

        /// <summary>
        /// Base currency of exchange operations
        /// </summary>
        private string BaseCurrency { get ; }

        /// <summary>
        /// Base currency of exchange operations
        /// </summary>
        private const string C_BaseCurrency = "USD" ;

        /// <summary>
        /// Currencies which possible to exchange
        /// </summary>
        private string TradingCurrencies { get ; }

        /// <summary>
        /// Currencies which possible to exchange
        /// </summary>
        private const string C_TradingCurrencies = "RUB,EUR,GBP,JPY" ;

        /// <summary>
        /// Pattern for report result of exchange rates analysis
        /// </summary>
        private string ResultReportPattern { get ; }

        /// <summary>
        /// Pattern for report result of exchange rates analysis
        /// </summary>
        private const string C_ResultReportPattern = " {0} обмен {1} {2} на {3}, обратный обмен {4} , прибыль {5} " ;

        /// <summary>
        /// Currency name when currency is undefined 
        /// </summary>
        private const string C_UndefinedCurrency = @"валюта не определена " ;

        /// <summary>
        /// Date name when date is undefined 
        /// </summary>
        private const string C_UndefinedDate = @"Дата не определена" ;

        /// <summary>
        /// Currencies for exchange
        /// </summary>
        private string [ ] Currencies { get ; }

        /// <summary>
        /// Trading currencies string separators
        /// </summary>
        private const string C_CurrenciesSeparators = "," ;

        /// <summary>
        /// Trading currencies string separators
        /// </summary>
        private string CurrenciesSeparators { get ; }

        /// <summary>
        /// </summary>
        /// <param name="greatestProfit"></param>
        /// <returns></returns>
        [ NotNull ]
        public string AnalysisResultToString
            (
            AnalysisResult greatestProfit )
        {

            var resultMessage = string.Empty ;
            var profitAsString = string.Empty ;
            var resultTargetCurrency = string.Empty ;
            var resultSourceCurrency = string.Empty ;
            var requestIntervalFinishAsString = string.Empty ;
            var requestIntervalStartAsString = string.Empty ;
            var sourceAmountAsString = string.Empty ;

            if ( greatestProfit != null )
            {
                sourceAmountAsString = greatestProfit.Amount.ToString
                    (
                        CultureInfo.InvariantCulture ) ;

                var isOpenDateDefined = greatestProfit.Open != DateTime.MinValue ;
                requestIntervalStartAsString = isOpenDateDefined
                                                   ? greatestProfit.Open.ToShortDateString ( )
                                                   : DealHandler.C_UndefinedDate
                    ;

                var isCloseDateDefined = greatestProfit.Close != DateTime.MinValue ;
                requestIntervalFinishAsString = isCloseDateDefined
                                                    ? greatestProfit.Close.ToShortDateString ( )
                                                    : DealHandler.C_UndefinedDate
                    ;

                var isSourceDefined = !string.IsNullOrEmpty
                                           (
                                               greatestProfit.Source ) ;
                resultSourceCurrency = isSourceDefined
                                           ? greatestProfit.Source
                                           : DealHandler.C_UndefinedCurrency ;

                var isTargetDefined = !string.IsNullOrEmpty
                                           (
                                               greatestProfit.Target ) ;
                resultTargetCurrency = isTargetDefined
                                           ? greatestProfit.Target
                                           : DealHandler.C_UndefinedCurrency ;

                var profit = greatestProfit.Profit ;
                profitAsString =
                    double.IsNaN
                        (
                            profit )
                        ? @"значение не определено"
                        : Math.Round
                              (
                                  profit ,
                                  2 ,
                                  MidpointRounding.ToEven )
                                .ToString
                              (
                                  CultureInfo.InvariantCulture ) ;
            }

            var resultReportPattern = this.ResultReportPattern ;

            if ( !string.IsNullOrEmpty
                      (
                          resultReportPattern ) )
            {
                resultMessage = string.Format
                    (
                        resultReportPattern ,
                        requestIntervalStartAsString ,
                        sourceAmountAsString ,
                        resultSourceCurrency ,
                        resultTargetCurrency ,
                        requestIntervalFinishAsString ,
                        profitAsString ) ;
            }

            return resultMessage ;
        }

        /// <summary>
        /// </summary>
        /// <param name="exchangeRatesList"></param>
        /// <returns></returns>
        private List < DealSuggestion > GetSuggestions
            (
            List < ExchangeRates > exchangeRatesList )
        {
            var currenciesExchanges = new CurrenciesExchanges
                (
                exchangeRatesList ,
                this.Currencies ,
                this.BaseCurrency
                ) ;

            var suggestions = currenciesExchanges.GetDealsSuggestion ( ) ;
            return suggestions ;
        }


        /// <summary>
        /// </summary>
        /// <param name="beginsDate"></param>
        /// <param name="endsDate"></param>
        /// <returns></returns>
        private List < ExchangeRates > GetExchangesRates
            (
            DateTime beginsDate ,
            DateTime endsDate )
        {
            var fixerExchangeRates = FixerHandler.GetFixerExchangeRates
                (
                    beginsDate ,
                    endsDate ) ;

            var exchangeRatesList = ( from fixerExchangeRate in fixerExchangeRates
                                      select new ExchangeRates ( fixerExchangeRate ) ).ToList ( ) ;
            return exchangeRatesList ;
        }

        /// <summary>
        /// </summary>
        /// <param name="suggestions"></param>
        /// <param name="baseCurrencyAmount"></param>
        /// <returns></returns>
        private AnalysisResult AnalysisSuggestions
            (
            List < DealSuggestion > suggestions ,
            double baseCurrencyAmount )
        {
            var currenciesDeals = new CurrenciesDeals
                (
                this.Currencies ,
                this.BaseCurrency
                ) ;

            currenciesDeals.AssumeSuggestions
                (
                    suggestions ,
                    baseCurrencyAmount ) ;

            var greatestProfit = currenciesDeals.GetDealWithGreatestProfit ( ) ;
            return greatestProfit ;
        }

        /// <summary>
        /// </summary>
        /// <param name="beginsDate"></param>
        /// <param name="endsDate"></param>
        /// <param name="baseCurrencyAmount"></param>
        /// <returns></returns>
        [ NotNull ]
        public CurrencyDealAnalisysResult GetDealWithGreatestProfit
            (
            DateTime beginsDate ,
            DateTime endsDate ,
            double baseCurrencyAmount
            )
        {

            var analysisData = new CurrencyDealAnalisysResult ( ) ;

            var exchangeRatesList = this.GetExchangesRates
                (
                    beginsDate ,
                    endsDate ) ;

            var suggestions = this.GetSuggestions
                (
                    exchangeRatesList ) ;

            var greatestProfit = this.AnalysisSuggestions
                (
                    suggestions ,
                    baseCurrencyAmount ) ;

            analysisData.ExchangeRatesList = exchangeRatesList ;
            analysisData.GreatestProfit = greatestProfit ;
            analysisData.Currencies = this.Currencies;

            return analysisData ;
        }

    }
}
