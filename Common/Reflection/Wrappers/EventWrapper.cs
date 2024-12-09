using System;
using System.Reflection;

namespace Common.Reflection;

internal class EventWrapper // for use with publicized assemblies
{
    private readonly object obj;
    private readonly EventInfo eventInfo;
    private MulticastDelegate eventDelegate;
    private MethodInfo adder, remover;

    public EventWrapper(EventInfo eventInfo, object obj = null)
    {
        this.obj = obj;
        this.eventInfo = eventInfo;
    }

    public void Add<D>(D eventDelegate)
    {
        Add(obj, eventDelegate);
    }

    public void Add<D>(object obj, D eventDelegate)
    {
        // $"EventWrapper.add for {eventInfo.Name}".logDbg();
        adder ??= eventInfo?.GetAddMethod();
        adder?.Invoke(obj, [eventDelegate]);
    }

    public void Remove<D>(D eventDelegate)
    {
        Remove(obj, eventDelegate);
    }

    public void Remove<D>(object obj, D eventDelegate)
    {
        // $"EventWrapper.remove for {eventInfo.Name}".logDbg();
        remover ??= eventInfo?.GetRemoveMethod();
        remover?.Invoke(obj, [eventDelegate]);
    }

    public void Raise(params object[] eventParams) // only for initial 'obj' for now
    {
        eventDelegate ??= eventInfo.DeclaringType.Field(eventInfo.Name)?.GetValue(obj) as MulticastDelegate;
        eventDelegate?.GetInvocationList().ForEach(dlg => dlg.Method.Invoke(dlg.Target, eventParams));
    }
}