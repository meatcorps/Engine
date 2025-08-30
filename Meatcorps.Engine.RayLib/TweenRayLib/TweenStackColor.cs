using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.TweenRayLib;

public class TweenStackColor
{
    private readonly TweenStack _stackR = new();
        private readonly TweenStack _stackG = new();
        private readonly TweenStack _stackB = new();
        private readonly TweenStack _stackA = new();

        public TweenStackColor Register(EaseType easeType, float normalOffset, float normalDuration)
        {
            _stackR.Register(easeType, normalOffset, normalDuration);
            _stackG.Register(easeType, normalOffset, normalDuration);
            _stackB.Register(easeType, normalOffset, normalDuration);
            _stackA.Register(easeType, normalOffset, normalDuration);
            return this;
        }

        public TweenStackColor Register(EaseType easeType, float duration, float totalDuration, float durationOffset)
        {
            _stackR.Register(easeType, duration, totalDuration, durationOffset);
            _stackG.Register(easeType, duration, totalDuration, durationOffset);
            _stackB.Register(easeType, duration, totalDuration, durationOffset);
            _stackA.Register(easeType, duration, totalDuration, durationOffset);
            return this;
        }
        
        public TweenStackColor FromKeyframes(params (float at, Color value, EaseType ease)[] keys)
        {
            if (keys == null || keys.Length < 2)
                throw new ArgumentException("Need at least two keyframes", nameof(keys));

            // Clear underlying stacks and seed with first key’s channels.
            _stackR.AssignFromValue(keys[0].value.R);
            _stackG.AssignFromValue(keys[0].value.G);
            _stackB.AssignFromValue(keys[0].value.B);
            _stackA.AssignFromValue(keys[0].value.A);

            for (var i = 0; i < keys.Length - 1; i++)
            {
                var from = keys[i];
                var to   = keys[i + 1];
                var segmentDuration = MathF.Max(1e-6f, to.at - from.at);

                // Register identical segment timing/ease for all channels
                Register(to.ease, normalOffset: from.at, normalDuration: segmentDuration);

                // Assign "to" values for this segment
                _stackR.AssignToValue(to.value.R);
                _stackG.AssignToValue(to.value.G);
                _stackB.AssignToValue(to.value.B);
                _stackA.AssignToValue(to.value.A);
            }

            return this;
        }

        public TweenStackColor FromDurationInMilliseconds(
            Color startValue,
            float totalDurationInMilliseconds,
            params (float durationInMilliseconds, Color value, EaseType ease)[] keys)
        {
            if (totalDurationInMilliseconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(totalDurationInMilliseconds), "Total duration must be > 0.");

            var normalized = new (float at, Color value, EaseType ease)[keys.Length + 1];

            normalized[0].at = 0f;
            normalized[0].value = startValue;
            normalized[0].ease = EaseType.Linear;

            var currentOffset = 0f;

            for (var i = 0; i < keys.Length; i++)
            {
                var segment = keys[i];
                if (segment.durationInMilliseconds < 0f)
                    throw new ArgumentOutOfRangeException(nameof(keys), "Duration cannot be negative.");

                var normalizedDuration = segment.durationInMilliseconds / totalDurationInMilliseconds;
                currentOffset += normalizedDuration;

                normalized[i + 1].at = currentOffset;
                normalized[i + 1].value = segment.value;
                normalized[i + 1].ease = segment.ease;
            }

            if (!currentOffset.EqualsSafe(1f))
                throw new ArgumentException("Total duration and keyframes duration mismatch.");

            return FromKeyframes(normalized);
        }

        public TweenStackColor AssignFromValue(Color value)
        {
            _stackR.AssignFromValue(value.R);
            _stackG.AssignFromValue(value.G);
            _stackB.AssignFromValue(value.B);
            _stackA.AssignFromValue(value.A);
            return this;
        }

        public TweenStackColor AssignToValue(Color value)
        {
            _stackR.AssignToValue(value.R);
            _stackG.AssignToValue(value.G);
            _stackB.AssignToValue(value.B);
            _stackA.AssignToValue(value.A);
            return this;
        }

        public TweenStackColor Assign(Color from, Color to)
        {
            AssignFromValue(from);
            AssignToValue(to);
            return this;
        }

        public Color Lerp(float normal)
        {
            // Clamp to byte with rounding
            static byte ToByte(float v) => (byte)Math.Clamp((int)MathF.Round(v), 0, 255);

            return new Color(
                ToByte(_stackR.Lerp(normal)),
                ToByte(_stackG.Lerp(normal)),
                ToByte(_stackB.Lerp(normal)),
                ToByte(_stackA.Lerp(normal))
            );
        }
}