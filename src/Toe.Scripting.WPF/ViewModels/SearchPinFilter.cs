using System.Collections.Generic;
using System.Linq;

namespace Toe.Scripting.WPF.ViewModels
{
    public class SearchPinFilter
    {
        private readonly bool isExecutionPin;
        private readonly bool isInputPin;
        private readonly string type;

        public SearchPinFilter(bool isExecutionPin, bool isInputPin, string type)
        {
            this.isExecutionPin = isExecutionPin;
            this.isInputPin = isInputPin;
            this.type = type;
        }

        public SearchPinFilter(PinViewModel pin, string type)
        {
            isExecutionPin = pin.IsExecutionPin;
            isInputPin = !pin.IsInputPin;
            Pin = pin;
            this.type = type;
        }

        public PinViewModel Pin { get; }

        public override string ToString()
        {
            return string.Format("Type:{2} Exec: {0}, Input {1}", isExecutionPin, isInputPin, type);
        }

        public bool IsMatch(FactoryViewModel factory)
        {
            if (isExecutionPin)
                if (isInputPin)
                    return factory.HasEnterPins;
                else
                    return factory.HasExitPins;

            IEnumerable<string> pinTypes;
            if (isInputPin)
                pinTypes = factory.InputTypes;
            else
                pinTypes = factory.OutputTypes;
            return pinTypes.Any(_ => _ == type || _ == null);
        }

        public IEnumerable<PinViewModel> FindMatchingPins(NodeViewModel nodeViewModel)
        {
            if (Pin != null && nodeViewModel == Pin.Node)
                return Enumerable.Empty<PinViewModel>();

            IList<PinViewModel> pins;
            if (isInputPin)
            {
                if (isExecutionPin)
                    pins = nodeViewModel.EnterPins;
                else
                    pins = nodeViewModel.InputPins;
            }
            else
            {
                if (isExecutionPin)
                    pins = nodeViewModel.ExitPins;
                else
                    pins = nodeViewModel.OutputPins;
            }

            return pins.Where(_ => _.Type == type || type == null || _.Type == null);
        }
    }
}