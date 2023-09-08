using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ronin
{
  [GlobalClass]
  public partial class TeamGroup : Node3D
  {
    [Export]
    private Godot.Collections.Array<Person> _teamMembers = new();

    public void AddMember(Person person)
    {
      if (_teamMembers.Contains(person))
        return;
      AddChild(person);
      _teamMembers.Add(person);
    }

    public void RemoveMember(Person target)
    {
      IEnumerable<Person> expressionValid =
        from member in _teamMembers
        where member != target
        select member;

      Godot.Collections.Array<Person> newTeamMembers = new();

      foreach (Person member in expressionValid)
      {
        newTeamMembers.Add(member);
      }

      _teamMembers = newTeamMembers;
    }

    public Godot.Collections.Array<Person> GetMembers()
    {
      return _teamMembers;
    }

    public Person? GetMember(int index)
    {
      if (index < 0 || index >= _teamMembers.Count)
        return null;
      return _teamMembers.ElementAt(index);
    }
  }
}
