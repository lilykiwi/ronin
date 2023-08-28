using System;
using Godot;

namespace Ronin
{
  [Tool]
  public partial class WorldManager : WorldEnvironment
  {
    private float _rotationSpeed = 0.1f;

    [Export]
    public float RotationSpeed
    {
      get => _rotationSpeed;
      set => this.SetValue(ref _rotationSpeed, value);
    }

    public override void _Ready()
    {
      base._Ready();
    }

    public override void _Process(double delta)
    {
      base._Process(delta);

      if (Engine.IsEditorHint())
      {
        return;
      }
    }
  }
}
