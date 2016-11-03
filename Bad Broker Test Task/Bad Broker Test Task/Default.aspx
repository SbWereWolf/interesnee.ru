<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Bad_Broker_Test_Task.Default" Theme="" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Bad Broker Test Task</title>
    <link href="bootstrap/css/bootstrap.css" rel="stylesheet" />
    <script type="text/javascript">

        var C_EmptyString = "<%= string.Empty %>";

        var C_ResultLabelClass = "label-success";

        var C_ResultTableClass = "table";
        var C_ResultTableHeaderClass = "lead";
        var C_ExchangeRowClass = "alert-danger";
        var C_ExchangeBackRowClass = "alert-success";

        var C_Table = "TABLE";
        var C_Label = "LABEL";
        var C_TableRow = "TR";
        var C_TableHeader = "TH";
        var C_TableDate = "TD";

        function submitParameters() {


            var firstDay = new Date();
            var lastDay = new Date();
            var amount = 0;

            var errorMessage = C_EmptyString;

            var firstDayElementId = "<%= this.IntervalFirstDayCalendar2 != null ? this.IntervalFirstDayCalendar2.ClientID : string.Empty %>";
            var lastDayElementId = "<%= this.IntervalLastDayCalendar2 != null ? this.IntervalLastDayCalendar2.ClientID : string.Empty %>";
            var amount1ElementId = "<%= this.BaseCurrencyAmountTextBox2 != null ? this.BaseCurrencyAmountTextBox2.ClientID : string.Empty %>";

            if (firstDayElementId === C_EmptyString) {
                errorMessage = errorMessage + " First day page control not found . ";
            } else {
                firstDay = new Date(document.getElementById(firstDayElementId).value);
            }
            if (lastDayElementId === C_EmptyString) {
                errorMessage = errorMessage + " Last day page control not found . ";
            } else {
                lastDay = new Date(document.getElementById(lastDayElementId).value);
            }
            if (amount1ElementId === C_EmptyString) {
                errorMessage = errorMessage + " Amount of base currency page control not found . ";
            } else {
                amount = document.getElementById(amount1ElementId).value;
            }

            var firstDayInms = firstDay.getTime();
            var isInvalidFirstDay = isNaN(firstDayInms);
            if (isInvalidFirstDay) {
                errorMessage = errorMessage + " First day of period is invalid . ";
            }
            var lastDayInms = lastDay.getTime();
            var isInvalidLastDay = isNaN(lastDayInms);
            if (isInvalidLastDay) {
                errorMessage = errorMessage + " Last day of period is invalid . ";
            }

            if (amount <= 0) {
                errorMessage = errorMessage + " Amount must be above zero . ";
            }

            /* one day = hours * minutes * seconds * milleseconds */
            var oneDayOffset = (24 * 60 * 60 * 1000);

            if (firstDay > lastDay) {
                var swap = firstDay;
                firstDay = lastDay;
                lastDay = swap;
            }
            var difference = Math.round((lastDay.getTime() - firstDay.getTime()) / oneDayOffset);
            var oneDay = 1;
            var twoMonthInDays = 60;
            if (difference < oneDay
                || difference > twoMonthInDays) {
                errorMessage = errorMessage + " Dates period is " + difference + " days . Dates period must be above zero and less or equal then 60 days . ";
            }

            if (errorMessage !== C_EmptyString) {
                alert(errorMessage);
            } else {
                var service = new window.Bad_Broker_Test_Task.CurrencyExchangeService();
                service.DoWork2(firstDay, lastDay, amount, onSuccess2, null, null);
            }
        }

        function onSuccess2(result) {

            var gP = result.GreatestProfit;

            var openDate = printDate(gP.Open);
            var closeDate = printDate(gP.Close);

            var resultAsString =
                " Deal open date is " + openDate
                    + " , exchange amount is " + gP.Amount
                    + " of  " + gP.Source
                    + " for " + gP.Target
                    + " , exchange back at " + closeDate
                    + " with " + gP.Profit.toFixed(2)
                    + " of " + gP.Source
                    + " profit . ";

            var dateColumnIndex = 0;

            var currenciesArray = result.Currencies;
            var currenciesCount = currenciesArray.length;

            var resultHeader = new Array();
            var firstIndex = 0;
            var hIndex = firstIndex;
            resultHeader[hIndex] = " Rates date ";
            dateColumnIndex = hIndex;
            hIndex++;
            resultHeader[hIndex] = " Base currency ";
            hIndex++;
            var n;
            for (n = 0; n < currenciesCount; n++) {
                resultHeader[hIndex] = " " + currenciesArray[n] + " ";
                hIndex++;
            }

            var resultRows = new Array();

            var exchangeRatesArray = result.ExchangeRatesList;
            var rIndex;
            for (rIndex = 0; rIndex < exchangeRatesArray.length; rIndex++) {
                var exchange = exchangeRatesArray[rIndex];
                var ratesDate = exchange.RatesDate;
                var base = exchange.Base;
                var rates = exchange.DatesRates;

                var currenciesHash = new Object();
                for (var r = 0; r < rates.length; r++) {
                    currenciesHash[rates[r].Key] = rates[r].Value;
                }

                resultRows[rIndex] = new Array();

                var cIndex = firstIndex;
                resultRows[rIndex][cIndex] = printDate(ratesDate);
                cIndex++;
                resultRows[rIndex][cIndex] = base;
                cIndex++;
                for (var e = 0; e < currenciesCount; e++) {
                    resultRows[rIndex][cIndex] = currenciesHash[currenciesArray[e]];
                    cIndex++;
                }
            }


            var exhangesRatesElementId = "<%= this.ExhangesRatesDivision != null ? this.ExhangesRatesDivision.ClientID : string.Empty %>";
            purgeNode(exhangesRatesElementId);

            if (exhangesRatesElementId !== C_EmptyString) {
                var resultDivElement = document.getElementById(exhangesRatesElementId);

                var resultLabelElement = document.createElement(C_Label);

                resultLabelElement.className = C_ResultLabelClass;
                resultLabelElement.appendChild(document.createTextNode(resultAsString));
                resultDivElement.appendChild(resultLabelElement);

                var resultTableElement = document.createElement(C_Table);

                resultTableElement.className = C_ResultTableClass;

                var tableHeaderElement = document.createElement(C_TableRow);
                for (var rh = 0; rh < resultHeader.length; rh++) {
                    addThElementWithText(tableHeaderElement, resultHeader[rh]);
                }
                resultTableElement.appendChild(tableHeaderElement);

                for (var rr = 0; rr < resultRows.length; rr++) {
                    var tableRowElement = document.createElement(C_TableRow);

                    var resultSingleRow = resultRows[rr];
                    for (var sr = 0; sr < resultSingleRow.length; sr++) {
                        addTdElementWithText(tableRowElement, resultSingleRow[sr]);
                    }

                    var dateColumn = resultSingleRow[dateColumnIndex];
                    if (dateColumn === openDate ) {
                        tableRowElement.className = C_ExchangeRowClass;
                    } else {
                        if (dateColumn === closeDate ) {
                            tableRowElement.className = C_ExchangeBackRowClass;
                        }
                    }

                    resultTableElement.appendChild(tableRowElement);
                }

                resultDivElement.appendChild(resultTableElement);
            }
        }

        function printDate(aDate) {
            var dateString = aDate.getFullYear()
                + "-" + (aDate.getMonth() + 1)
                + "-" + aDate.getDate();
            return dateString;
        }

        function purgeNode(nodeElementId) {
            if (nodeElementId !== C_EmptyString) {
                var node = document.getElementById(nodeElementId);
                node.Text = C_EmptyString;
                while (node.hasChildNodes()) {
                    node.removeChild(node.firstChild);
                }
            }
        }

        function addThElementWithText(parentElement, childText) {

            var tableHeaderElement = document.createElement(C_TableHeader);
            tableHeaderElement.appendChild(document.createTextNode(childText));

            tableHeaderElement.className = C_ResultTableHeaderClass;
            parentElement.appendChild(tableHeaderElement);
        }

        function addTdElementWithText(parentElement, childText) {

            var tableHeaderElement = document.createElement(C_TableDate);
            tableHeaderElement.appendChild(document.createTextNode(childText));
            parentElement.appendChild(tableHeaderElement);
        }

    </script>
</head>
<body>
    <form id="CalculateBestDealForm" runat="server">
        <div class="container">
            <asp:ScriptManager ID="PageScriptManager" runat="server">
                <Services>
                    <asp:ServiceReference Path="CurrencyExchangeService.svc" />
                </Services>
            </asp:ScriptManager>
            <label id="CurrencyAmountLabel2" runat="server" text="">Currency amount ( USD ) </label>
            <br />
            <input type="number" id="BaseCurrencyAmountTextBox2" runat="server" causesvalidation="False" value="0" />
            <br />
            <label id="IntervalAnalysisFirstDayLabel2" runat="server">Interval analysis , first day</label><br />
            <input type="date" id="IntervalFirstDayCalendar2" runat="server" />
            <br />
            <label id="IntervalAnalysisLastDayLabel2" runat="server">Interval analysis , last day</label>
            <br />
            <input type="date" id="IntervalLastDayCalendar2" runat="server" />
            <br />
            <input type="button" id="DoAnalysisButton2" runat="server" value="Submit" onclick="submitParameters()" />
            <br />
        </div>
        <div id="ExhangesRatesDivision" runat="server" class="container"></div>
    </form>
</body>
</html>
