using Windows.Globalization.NumberFormatting;

namespace PDV.UI.WinUI3.Helpers
{
    public static class NumberFormatHelper
    {
        private static DecimalFormatter? _currencyFormatter;

        public static DecimalFormatter CurrencyFormatter
        {
            get
            {
                if (_currencyFormatter == null)
                {
                    _currencyFormatter = new DecimalFormatter
                    {
                        IntegerDigits = 1,
                        FractionDigits = 2,
                        NumberRounder = new IncrementNumberRounder
                        {
                            Increment = 0.01,
                            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
                        }
                    };
                }
                return _currencyFormatter;
            }
        }
    }
}
