using Godot;

namespace Ronin
{
  [GlobalClass, Tool]
  public partial class TeamManager : Node3D
  {
    Callable[] GetToolButtons() =>
      new Callable[] { new Callable(this, nameof(_Ready)) };

    private string _teamName = "Ronin";
    private PlayerView? _playerView = null;
    private TeamGroup? _teamGroup = null;

    public string TeamName
    {
      get => _teamName;
      set => this.SetValue(ref _teamName, value);
    }

    public override void _Ready()
    {
      _teamGroup = this.GetOrCreate<TeamGroup>();
      _playerView = this.GetOrCreate<PlayerView>(); // DANGEROUS ATM :D

      // placeholder initialiser for now
      _teamGroup.AddMember(new Person(Position: _playerView.Position));
    }

    /// <summary>SetTarget</summary>
    /// <remarks>PlayerView calls this to update the nav target for the members
    /// of a given team.</remarks>
    /// <param name="target"></param>
    public void SetTarget(Vector3 target)
    {
      if (_teamGroup == null)
        return;
      foreach (Person member in _teamGroup.GetMembers())
        member.SetTarget(target);
    }
  }
}
