using System.Numerics;
using Meatcorps.Engine.RayLib.Interfaces;
using R3D_cs;

namespace Meatcorps.Engine.RayLib.R3d.Interfaces;

public interface ICamera3D: ICamera
{
    public Vector3 Position3D { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3? Target3D { get; set; }
    public bool Orthographic { get; set; }
    public ref R3D_cs.Camera Native { get; }
    public Vector3 Target3DUpDirection { get; set; }
}