using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Hardware.Controllers.Constants;
using Meatcorps.Engine.Hardware.Controllers.Enums;

namespace Meatcorps.Engine.Hardware.Controllers;

public class ControllerInput: GenericInput
{
    private readonly Func<ControllerType> _type;
    private readonly string _originalLabel;

    public override string Label
    {
        get
        {
            switch (_type())
            {
                case ControllerType.XBox:
                case ControllerType.Steam:
                case ControllerType.Nintendo:
                case ControllerType.Other:
                    switch (_originalLabel)
                    {
                        case ButtonConstants.MiddleLeft:
                            return ButtonConstants.Back;
                        case ButtonConstants.MiddleRight:
                            return ButtonConstants.Start;
                        case ButtonConstants.RightFaceDown:
                            return ButtonConstants.A;
                        case ButtonConstants.RightFaceUp:
                            return ButtonConstants.Y;
                        case ButtonConstants.RightFaceLeft:
                            return ButtonConstants.X;
                        case ButtonConstants.RightFaceRight:
                            return ButtonConstants.B;
                        default:
                            return _originalLabel;
                    }
                case ControllerType.PlayStation:
                    switch (_originalLabel)
                    {
                        case ButtonConstants.MiddleLeft:
                            return ButtonConstants.Share;
                        case ButtonConstants.MiddleRight:
                            return ButtonConstants.Options;
                        case ButtonConstants.RightFaceDown:
                            return ButtonConstants.Cross;
                        case ButtonConstants.RightFaceUp:
                            return ButtonConstants.Triangle;
                        case ButtonConstants.RightFaceLeft:
                            return ButtonConstants.Square;
                        case ButtonConstants.RightFaceRight:
                            return ButtonConstants.Circle;
                        case ButtonConstants.LeftBumper:
                            return ButtonConstants.L1;
                        case ButtonConstants.LeftTrigger:
                            return ButtonConstants.L2;
                        case ButtonConstants.RightBumper:
                            return ButtonConstants.R1;
                        case ButtonConstants.RightTrigger:
                            return ButtonConstants.R2;
                        default:
                            return _originalLabel;
                    }
                default:
                    return _originalLabel;
            }
        }
    }

    public ControllerInput(Func<ControllerType> type, Func<float> pressedFunc, string label) : base(pressedFunc, label)
    {
        _originalLabel = label;
        _type = type;
    }
}