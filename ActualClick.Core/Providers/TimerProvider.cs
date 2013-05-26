using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinBuyer.B2B.DealFinder.Core.Providers.Market
{
    public class TimerProvider
    {
        private DateTime? _startTime = null;
        private readonly DealFinderContext _cotext = null;
        private readonly string _operationName = null;

        public TimerProvider(DealFinderContext context, string operationName)
        {
            _operationName = operationName;
            _cotext = context;
            _startTime = DateTime.Now;
        }

        public void StopTimer()
        {
            var interval = DateTime.Now - _startTime.Value;

            _cotext.AddTimer(_operationName, string.Format("{0:0}", interval.TotalMilliseconds));
        }
    }
}
