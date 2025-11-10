using System.Numerics;
using System.Text;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;
using Meatcorps.Engine.Hardware.Controllers.Mapper;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Input;
using Meatcorps.Engine.RayLib.Resources;
using Microsoft.Extensions.Logging;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

// Change the MainScene.cs load this example

public class RawControllerTest : BaseGameObject
{
    private ControllerInputMapper<ControllerInputEnum> _controllerInputMapper;
    private IControllerDeviceManager _controllerDeviceManager;
    private TextManager<DefaultFont> _fonts;
    private ILogger<RawControllerTest> _logger;
    private string _previousText = "";
    protected override void OnInitialize()
    {
        
        _logger = LoggingService.GetLogger<RawControllerTest>();
        Camera = CameraLayer.UI;
        
        //if (GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!.GetOrDefault("Debug", "UseRayLibController", true))
            //_controllerDeviceManager = new RayLibControllerDeviceManager();
        //else
            _controllerDeviceManager = new SDLControllerDeviceManager();
        
        _controllerInputMapper = new ControllerInputMapper<ControllerInputEnum>(_controllerDeviceManager);
        _fonts = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        
        // Just everything :)
        foreach (var type in Enum.GetValues<ControllerInputEnum>())
        {
            _controllerInputMapper.Map(type, type);
        }
        
        _controllerDeviceManager.AssignDevice(1, 0);
        _controllerDeviceManager.AssignDevice(2, 1);
        _controllerDeviceManager.AssignDevice(3, 2);
        _controllerDeviceManager.AssignDevice(4, 3);
    }

    protected override void OnPreUpdate(float deltaTime)
    {
        _controllerInputMapper.PreUpdate(deltaTime);
        base.OnPreUpdate(deltaTime);
    }

    protected override void OnUpdate(float deltaTime)
    {
        for (var player = 1; player <= 4; player++)
        {
            if (_controllerInputMapper.GetState(player, ControllerInputEnum.AorCross).IsPressed)
            {
                _controllerInputMapper.Rumble(player, 1, 0, 1);
            }
            if (_controllerInputMapper.GetState(player, ControllerInputEnum.BorCircle).IsPressed)
            {
                _controllerInputMapper.Rumble(player, 0, 1, 1);
            }
        }
    }

    protected override void OnDraw()
    {
        var outputViewer = new StringBuilder();
        var counter = 0;
        for (var player = 1; player <= 4; player++)
        {
            outputViewer.Clear();
            if (_controllerDeviceManager.IsDeviceAssigned(player))
            {
                outputViewer.AppendLine("AXIS1: " + _controllerInputMapper.GetAxis(player, 1).ToString());
                outputViewer.AppendLine("AXIS2: " + _controllerInputMapper.GetAxis(player, 2).ToString());
                var count = 0;
                var type = _controllerDeviceManager.GetDevice(player)?.Type ?? ControllerType.Other;
                foreach (var controllerType in Enum.GetValues<ControllerInputEnum>())
                {
                    var state = _controllerInputMapper.GetState(player, controllerType);
                    if (Math.Abs(state.Normalized) > 0.1f)
                    {
                        outputViewer.AppendLine(controllerType + ": " + state.Normalized.ToString("F2"));
                        count++;
                    }
                }
                var text = _previousText;

                text = "> " + player + " (" + type + ") " +
                       outputViewer.ToString().Replace("\n", ",").Replace("\r", "");
                if (_previousText != text)
                {
                   // _logger.LogInformation(text);
                }

                _previousText = text;

                Raylib_cs.Raylib.DrawTextEx(_fonts.GetFont(), "> " + player + "(" + type + ")\n" + outputViewer.ToString(), new Vector2(16 + counter, 16), 16f, 1f, Color.White);
            }

            counter += 400;
        }
        
        
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}