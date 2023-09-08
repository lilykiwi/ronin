using System;
using Godot;

namespace Ronin
{
  [GlobalClass, Tool]
  public partial class Person : RigidBody3D
  {
    /// <summary> This is all placeholders atm. </summary>

    public string FirstName = "Ronin";
    public string LastName = "Ronin";
    public string FullName => $"{FirstName} {LastName}";
    public string NickName = "Ronin";
    public string Title = "";
    public string Suffix = "";
    public string Description = "";
    public string Notes = "";
    public string VoiceAccent = "";
    public string VoiceLanguage = "";

    public int Age = 18;
    public int BodyHeight = 180;
    public int BodyWeight = 80;

    // pronouns
    public string Subj = "they";
    public string SubjCon = "they're";
    public string Obj = "them";
    public string Poss = "their";
    public string Refl = "themself";
    public string AltRefl = "themselves";

    // stats teehee
    public int Talking = 0;
    public int Analyzing = 0;
    public int Negotiating = 0;
    public int Attacking = 0;
    public int Defending = 0;
    public int Healing = 0;
    public int Sneaking = 0;
    public int Moving = 0;
    public int Exploring = 0;
    public int Crafting = 0;
    public int Building = 0;
    public int Learning = 0;

    // stuff
    public int Health = 100;
    public int MaxHealth = 100;
    public int CarryCapacity = 100;
    public int MaxCarryCapacity = 100;
    public int Hunger = 0;
    public int MaxHunger = 100;
    public int Thirst = 0;
    public int MaxThirst = 100;
    public int Energy = 100;
    public int MaxEnergy = 100;

    public bool IsMoving = false;

    private NavigationAgent3D? _agent = null;
    private CollisionShape3D? _collider = null;
    private MeshInstance3D? _mesh = null;

    public Person(Vector3 Position)
    {
      this.Position = Position;
    }

    public override void _Ready()
    {
      base._Ready();

      _agent = this.GetOrCreate<NavigationAgent3D>();
      _agent.MaxSpeed = 6f;
      //
      _collider = this.GetOrCreate<CollisionShape3D>();
      _collider.Shape = new CapsuleShape3D() { Height = 1.8f, Radius = 0.25f, };
      //
      _mesh = this.GetOrCreate<MeshInstance3D>();
      _mesh.Mesh = new CapsuleMesh() { Height = 1.8f, Radius = 0.25f, };
      //
      this.FreezeMode = FreezeModeEnum.Static;
      this.Freeze = true;
    }

    public override void _PhysicsProcess(double delta)
    {
      if (Engine.IsEditorHint())
        return;
      if (_agent is null)
        GD.PushError($"_agent is null in {this.Name}");

      if (_agent!.IsTargetReachable() && !_agent.IsNavigationFinished())
      {
        GlobalPosition = GetMovePosition(delta, _agent);
      }
    }

    private Vector3 GetMovePosition(double delta, NavigationAgent3D agent)
    {
      Vector3 vectorDelta = GlobalPosition.MoveToward(
        agent.GetNextPathPosition(),
        (float)delta * agent.MaxSpeed
      );
      return vectorDelta;
    }

    public void SetTarget(Vector3 target)
    {
      if (_agent is null)
        return;

      GD.Print($"Setting target for {this.Name} to {target}");
      _agent.TargetPosition = target;
    }
  }
}
