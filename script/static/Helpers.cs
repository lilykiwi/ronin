using System.Linq;
using System.Runtime.CompilerServices;
using Godot;

namespace Ronin
{
  static class Helpers
  {
    public static T GetOrCreate<T>(this Node parent, string childName = "_")
      where T : Node, new()
    {
      if (childName == "_")
      {
        childName = typeof(T).Name;
      }

      T childNode = parent.GetNodeOrNull<T>(childName);

      if (childNode is null)
      {
        childNode = new T();
        childNode.Name = childName;
        parent.AddChild(childNode);
        childNode.Owner = parent.GetTree().EditedSceneRoot;
      }

      return childNode;
    }

    /// <summary>SetValue</summary>
    /// <remarks>Use this by calling Node.SetValue(...), rather than
    /// Helpers.SetValue(this, ...), for neatness :D</remarks>
    /// <param name="node">(Node) </param>
    /// <param name="current">(*) old value, typically private</param>
    /// <param name="newValue">(*) new value</param>
    /// <param name="name">[optional] (string) defaults to caller name</param>
    /// <param name="signal">[optional] (Signal) callback signal</param>
    /// <param name="args">[optional] (Variant[]) signal arguments</param>
    public static void SetValue<T>(
      this Node node,
      ref T? current,
      T? newValue,
      string? signal = null,
      [CallerMemberName] string name = "_",
      params Variant[] args
    )
    {
      if (current is null || newValue is null || !current.Equals(newValue))
      {
        current = newValue;
        Variant[] signalParams = args.Prepend(name).ToArray<Variant>();
        if (signal is not null)
          node.EmitSignal(signal, signalParams);
      }
    }
  }
}
