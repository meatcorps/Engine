namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// Wraps a value and tracks whether it has changed since it was last applied.
/// </summary>
/// <typeparam name="T">
/// The type of value being tracked.
/// </typeparam>
/// <remarks>
/// <para>
/// A <see cref="DirtyValue{T}"/> becomes dirty whenever its <see cref="Value"/> changes.
/// Calling <see cref="Apply"/> invokes the configured apply action only when the value is dirty.
/// </para>
/// <para>
/// When auto-apply is enabled, changes to <see cref="Value"/> and calls to
/// <see cref="ForceUpdate"/> are applied immediately.
/// </para>
/// </remarks>
public class DirtyValue<T> where T : notnull
{
    private T _value;

    /// <summary>
    /// Gets or sets the tracked value.
    /// </summary>
    /// <remarks>
    /// Assigning a value equal to the current value does nothing.
    /// When the value changes, the instance is marked dirty.
    /// If auto-apply is enabled, <see cref="Apply"/> is called immediately.
    /// </remarks>
    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;

            _value = value;
            _dirty = true;

            if (_autoApply)
                Apply();
        }
    }

    private bool _firstTime;
    private bool _dirty;
    private Action<T, bool> _applyAction;
    private readonly bool _autoApply;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirtyValue{T}"/> class.
    /// </summary>
    /// <param name="initialValue">
    /// The initial value to track.
    /// </param>
    /// <param name="applyAction">
    /// The action executed when the value is applied.
    /// The first argument contains the current value and the second argument is
    /// <see langword="true"/> for the first application and <see langword="false"/>
    /// for all subsequent applications.
    /// </param>
    /// <param name="autoApply">
    /// When <see langword="true"/>, changes and forced updates are applied immediately.
    /// When <see langword="false"/>, <see cref="Apply"/> must be called explicitly.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="initialValue"/> or <paramref name="applyAction"/> is
    /// <see langword="null"/>.
    /// </exception>
    public DirtyValue(T initialValue, Action<T, bool> applyAction, bool autoApply = false)
    {
        _value = initialValue;
        _applyAction = applyAction;
        _autoApply = autoApply;
        _firstTime = true;
    }

    /// <summary>
    /// Marks the value as dirty, causing the next call to <see cref="Apply"/> to invoke
    /// the configured apply action.
    /// </summary>
    /// <remarks>
    /// When auto-apply is enabled, the value is applied immediately.
    /// </remarks>
    public void ForceUpdate()
    {
        _dirty = true;

        if (_autoApply)
            Apply();
    }
    
    /// <summary>
    /// Gets or sets the value by reference.
    /// </summary>
    public ref T ValueRef => ref _value;

    /// <summary>
    /// Implicitly converts a <see cref="DirtyValue{T}"/> to its underlying value.
    /// </summary>
    /// <param name="value">
    /// The tracked value wrapper to convert.
    /// </param>
    /// <returns>
    /// The underlying <see cref="Value"/>.
    /// </returns>
    public static implicit operator T(DirtyValue<T> value)
    {
        return value.Value;
    }

    /// <summary>
    /// Applies the current value when it is dirty.
    /// </summary>
    /// <remarks>
    /// If the value is not dirty, this method does nothing.
    /// The apply action receives <see langword="true"/> on its first invocation and
    /// <see langword="false"/> on subsequent invocations.
    /// After a successful application, the value is marked clean.
    /// </remarks>
    public void Apply()
    {
        if (!_dirty)
            return;

        _applyAction(Value, _firstTime);
        _firstTime = false;
        _dirty = false;
    }
}