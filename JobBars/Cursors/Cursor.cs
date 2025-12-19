using JobBars.Data;
using JobBars.Helper;
using System;

namespace JobBars.Cursors {
    public enum CursorType {
        [System.ComponentModel.Description("无")]
        None,
        [System.ComponentModel.Description("GCD")]
        GCD,
        [System.ComponentModel.Description("咏唱时间")]
        CastTime,
        [System.ComponentModel.Description("MP恢复")]
        MpTick,
        [System.ComponentModel.Description("角色恢复")]
        ActorTick,
        [System.ComponentModel.Description("DoT恢复")]
        DoT_Tick,
        [System.ComponentModel.Description("静态圆圈")]
        StaticCircle,
        [System.ComponentModel.Description("静态圆环")]
        StaticRing,
        [System.ComponentModel.Description("状态时间")]
        StatusTime,
        [System.ComponentModel.Description("滑读")]
        Slidecast
    }

    public class Cursor {
        public static readonly CursorType[] ValidCursorType = ( CursorType[] )Enum.GetValues( typeof( CursorType ) );

        private readonly string Name;
        private readonly string InnerName;
        private readonly string OuterName;

        private CursorType InnerType;
        private CursorType OuterType;

        private ItemData InnerStatus;
        private ItemData OuterStatus;
        private float InnerStatusDuration;
        private float OuterStatusDuration;

        public Cursor( JobIds job, CursorType inner, CursorType outer ) {
            Name = $"{job}";
            InnerName = Name + "_Inner";
            OuterName = Name + "_Outer";

            InnerType = JobBars.Configuration.CursorType.Get( InnerName, inner );
            OuterType = JobBars.Configuration.CursorType.Get( OuterName, outer );

            InnerStatus = JobBars.Configuration.CursorStatus.Get( InnerName, new ItemData {
                Name = "[无]",
                Data = new Item()
            } );
            OuterStatus = JobBars.Configuration.CursorStatus.Get( OuterName, new ItemData {
                Name = "[无]",
                Data = new Item()
            } );
            InnerStatusDuration = JobBars.Configuration.CursorStatusDuration.Get( InnerName, 5f );
            OuterStatusDuration = JobBars.Configuration.CursorStatusDuration.Get( OuterName, 5f );
        }

        public float GetInner() => GetValue( InnerType, InnerStatus, InnerStatusDuration );
        public float GetOuter() => GetValue( OuterType, OuterStatus, OuterStatusDuration );

        private static float GetValue( CursorType type, ItemData status, float statusDuration ) => type switch {
            CursorType.None => 0,
            CursorType.GCD => UiHelper.GetGCD( out var _, out var _ ),
            CursorType.CastTime => UiHelper.GetCastTime( out var _, out var _ ),
            CursorType.MpTick => UiHelper.GetMpTick(),
            CursorType.ActorTick => UiHelper.GetActorTick(),
            CursorType.StaticCircle => 2, // just a placeholder value, doesn't actually matter
            CursorType.StaticRing => 1,
            CursorType.StatusTime => GetStatusTime( status, statusDuration ),
            CursorType.DoT_Tick => UiHelper.GetDoTTick(),
            CursorType.Slidecast => GetSlidecastTime(),
            _ => 0
        };

        private static float GetStatusTime( ItemData status, float statusDuration ) {
            if( statusDuration == 0 ) return 0;
            if( status.Data.Id == 0 ) return 0;
            var ret = ( UiHelper.PlayerStatus.TryGetValue( status.Data, out var value ) ? ( value.RemainingTime > 0 ? value.RemainingTime : value.RemainingTime * -1 ) : 0 ) / statusDuration;
            return Math.Min( ret, 1f );
        }

        private static float GetSlidecastTime() {
            if( JobBars.Configuration.GaugeSlidecastTime <= 0f ) return UiHelper.GetCastTime( out var _, out var _ );

            var isCasting = UiHelper.GetCurrentCast( out var currentTime, out var totalTime );
            if( !isCasting || totalTime == 0 ) return 0;
            var slidecastTime = totalTime - JobBars.Configuration.GaugeSlidecastTime;
            if( currentTime > slidecastTime ) return 0;
            return currentTime / slidecastTime;
        }

        public void Draw( string _ID ) {
            if( JobBars.Configuration.CursorType.Draw( $"内圈类型{_ID}", InnerName, ValidCursorType, InnerType, out var newInnerValue ) ) {
                InnerType = newInnerValue;
            }

            if( InnerType == CursorType.StatusTime ) {
                if( JobBars.Configuration.CursorStatus.Draw( $"内圈状态{_ID}", InnerName, UiHelper.StatusList, InnerStatus, out var newInnerStatus ) ) {
                    InnerStatus = newInnerStatus;
                }
                if( JobBars.Configuration.CursorStatusDuration.Draw( $"内圈状态持续时间{_ID}", InnerName, InnerStatusDuration, out var newInnerStatusDuration ) ) {
                    InnerStatusDuration = newInnerStatusDuration;
                }
            }

            if( JobBars.Configuration.CursorType.Draw( $"外圈类型{_ID}", OuterName, ValidCursorType, OuterType, out var newOuterValue ) ) {
                OuterType = newOuterValue;
            }

            if( OuterType == CursorType.StatusTime ) {
                if( JobBars.Configuration.CursorStatus.Draw( $"外圈状态{_ID}", OuterName, UiHelper.StatusList, OuterStatus, out var newOuterStatus ) ) {
                    OuterStatus = newOuterStatus;
                }
                if( JobBars.Configuration.CursorStatusDuration.Draw( $"外圈状态持续时间{_ID}", OuterName, OuterStatusDuration, out var newOuterStautsDuration ) ) {
                    OuterStatusDuration = newOuterStautsDuration;
                }
            }
        }
    }
}
