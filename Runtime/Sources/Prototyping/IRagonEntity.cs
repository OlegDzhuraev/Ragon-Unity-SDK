using NetStack.Serialization;
using Ragon.Common;

namespace Ragon.Client.Prototyping
{
  public interface IRagonEntity
  {
    public int Id { get; }
    public void ReplicateEvent<TEvent>(ushort eventCode, TEvent evnt, RagonEventMode eventMode = RagonEventMode.SERVER_ONLY) where TEvent : IRagonSerializable, new();
  }
}