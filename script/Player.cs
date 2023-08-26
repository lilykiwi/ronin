using System;
using Godot;

[Tool]
public partial class Player : CharacterBody3D
{
  /* ----------------------------------------------------------------------- */
  //
  //
  //
  /* ----------------------------------------------------------------------- */
  [ExportCategory("Sens")] /* -------------------- Sens -------------------- */
  [Export] public double Sensitivity = 0.0220048900; // degrees per dot
  [Export] public double DPI = 1000.0;               // dots per inch
  [Export] public double In_Per_360 = 16.36;         // inches per 360 deg
  // These 3 values are a triangle of sorts. If you know any 2, you can 
  // calculate the third. We can query for the DPI and set the sensitivity
  // based off of that. I'm presuming that 16.36 is a good suggestion for
  // users. This would be easier if the wider industry cared at all about
  // the math they're using and giving control to end users to configure it.
  
  [ExportCategory("Camera")] /* ------------------ Camera ------------------ */
  [Export] public Camera3D? Camera = null;
  [Export] public ViewTypes ViewType = ViewTypes.Perspective;
  [Export(PropertyHint.Range, "0, 150, 5")] public float FOV = 75f;
  [ExportGroup("Zoom")]
  [Export] public Vector2 ZoomLimits = new (0.1f, 30f);
  [Export] public float ZoomSpeed = 0.1f;
  [Export] public float Zoom = 10f;
  [Export] public ZoomTypes ZoomType = ZoomTypes.Linear;
  
  public bool Dragging = false;
  public Vector2 LastMousePosition = new(0, 0);

  public enum ViewTypes 
  {
    Perspective,
    Orthographic,
    Isometric,
  }
  public enum ZoomTypes
  {
    Linear,
    Exponential,
    Logarithmic,
  }

  [ExportCategory("Physics")] /* ----------------- Physics ----------------- */
  [Export] public float MoveSpeed = 5.0f;
  [Export] public float JumpVelocity = 4.5f;
  [Export] public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();


  /* ----------------------------------------------------------------------- */

  public override void _Ready()
  {
    
    if (HasNode("MainCamera"))
    {
      Camera = GetNode<Camera3D>("MainCamera");
    }
    else if (Camera == null)
    {
      Camera = new()
      {
        Name = "MainCamera",
        Position = new Vector3(0f, 1.6f, 3f),
        RotationDegrees = new Vector3(-30f, 0f, 0f),
      };
      AddChild(Camera);
      Camera.Owner = GetTree().EditedSceneRoot;
    }
  }

  /// <summary>_PhysicsProcess</summary>
  /// <remarks>Processes physics for the node. This is provided by Godot as an
  /// example for player movement. Below (in comments) is my original GDScript
  /// implementation in my FPS test that calculates degrees per inch based on
  /// DPI and sensitivity.</remarks>
  /// <param name="delta">(double) do we need double precision?</param>
  /// <returns>void</returns>
  public override void _PhysicsProcess(double delta)
  {
    
    if (Engine.IsEditorHint())
    {
      return;
    }
    
    Vector3 velocity = Velocity;

    // Add the gravity.
    if (!IsOnFloor())
      velocity.Y -= gravity * (float)delta;

    // Handle Jump.
    if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
      velocity.Y = JumpVelocity;

    // Get the input direction and handle the movement/deceleration.
    // As good practice, you should replace UI actions with custom gameplay actions.
    Vector2 inputDir = Input.GetVector(
      "MoveLeft",
      "MoveRight",
      "MoveForward",
      "MoveBackward"
    );
    Vector3 cameraAnglesRadians = new Vector3(
      x: Camera!.RotationDegrees.X / 180 * MathF.PI,
      y: Camera!.RotationDegrees.Y / 180 * MathF.PI,
      z: Camera!.RotationDegrees.Z / 180 * MathF.PI
    );
    
    Vector3 direction = new Vector3(
      x: inputDir.X * MathF.Cos(cameraAnglesRadians.Y)
       + inputDir.Y * MathF.Sin(cameraAnglesRadians.Y),
      y: 0,
      z: inputDir.X * MathF.Sin(-cameraAnglesRadians.Y)
       + inputDir.Y * MathF.Cos(cameraAnglesRadians.Y)
    ).Normalized();
    if (direction != Vector3.Zero)
    {
      velocity.X = direction.X * MoveSpeed;
      velocity.Z = direction.Z * MoveSpeed;
    }
    else
    {
      velocity.X = Mathf.MoveToward(Velocity.X, 0, MoveSpeed);
      velocity.Z = Mathf.MoveToward(Velocity.Z, 0, MoveSpeed);
    }

    Velocity = velocity;
    MoveAndSlide();
  }

  public override void _Process(double delta)
  {
    if (Camera == null)
      return;
    Vector3 cameraAngles = Camera.RotationDegrees;
    Vector3 cameraAnglesRadians = new Vector3(
      x: cameraAngles.X / 180 * MathF.PI,
      y: cameraAngles.Y / 180 * MathF.PI,
      z: cameraAngles.Z / 180 * MathF.PI
    );

    Vector3 IdealPosition = new (
      x: MathF.Sin(cameraAnglesRadians.Y) * MathF.Cos(cameraAnglesRadians.X) * Zoom,
      y: MathF.Sin(-cameraAnglesRadians.X) * Zoom,
      z: MathF.Cos(cameraAnglesRadians.Y) * MathF.Cos(cameraAnglesRadians.X) * Zoom
    );
    
    Camera.Position = IdealPosition;
  }

  /// <summary>_Input</summary>
  /// <remarks>Processes input.</remarks>
  /// <TODO>Move this to an input handler.</TODO>
  /// <param name="event"></param>
  /// <returns>void</returns>
  public override void _Input(InputEvent inputEvent)
  {
    base._Input(inputEvent);
    
    if (inputEvent is InputEventMouseButton EventMouseButton)
    {
      if (EventMouseButton.IsActionPressed("CameraRotate"))
      {
        Dragging = true;
        LastMousePosition = EventMouseButton.Position;
        Input.MouseMode = Input.MouseModeEnum.Captured;
      }
      if (EventMouseButton.IsActionReleased("CameraRotate"))
      {
        Dragging = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Input.WarpMouse(LastMousePosition);
      }
      
      if (EventMouseButton.IsAction("ZoomIn"))
      {
        Zoom = Mathf.Clamp(Zoom - ZoomSpeed, ZoomLimits.X, ZoomLimits.Y);
      }
      if (EventMouseButton.IsAction("ZoomOut"))
      {
        Zoom = Mathf.Clamp(Zoom + ZoomSpeed, ZoomLimits.X, ZoomLimits.Y);
      }
      
    }

    if (inputEvent is InputEventMouseMotion EventMouseMotion)
    {
      if (Dragging)
      {
        Camera!.RotationDegrees -= new Vector3(
          y: EventMouseMotion.Relative.X * (float)Sensitivity,
          x: EventMouseMotion.Relative.Y * (float)Sensitivity,
          z: 0
        );
        Camera!.RotationDegrees = new Vector3(
          Mathf.Clamp(Camera.RotationDegrees.X, -90, 90),
          Mathf.Wrap(Camera.RotationDegrees.Y, 0, 360),
          0
        );
      }
    }
  }
}
