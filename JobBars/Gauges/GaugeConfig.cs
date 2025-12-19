using Dalamud.Bindings.ImGui;
using JobBars.Data;
using JobBars.Gauges.Types;
using JobBars.Gauges.Types.Arrow;
using JobBars.Gauges.Types.Bar;
using JobBars.Gauges.Types.BarDiamondCombo;
using JobBars.Gauges.Types.Diamond;
using System;
using System.Linq;
using System.Numerics;

namespace JobBars.Gauges {
    public abstract class GaugeConfig {
        public readonly string Name;
        public GaugeVisualType Type { get; private set; }
        public GaugeTypeConfig TypeConfig { get; private set; }

        public bool Enabled { get; protected set; }
        public int Order { get; private set; }
        public float Scale { get; private set; }
        public bool HideWhenInactive { get; private set; }
        public int SoundEffect { get; private set; }
        public int CompletionSoundEffect { get; private set; }
        public Vector2 Position => JobBars.Configuration.GaugeSplitPosition.Get( Name );

        public static readonly GaugeCompleteSoundType[] ValidSoundType = ( GaugeCompleteSoundType[] )Enum.GetValues( typeof( GaugeCompleteSoundType ) );

        public GaugeConfig( string name, GaugeVisualType type ) {
            Name = name;
            Enabled = JobBars.Configuration.GaugeEnabled.Get( Name );
            Order = JobBars.Configuration.GaugeOrder.Get( Name );
            Scale = JobBars.Configuration.GaugeIndividualScale.Get( Name );
            HideWhenInactive = JobBars.Configuration.GaugeHideInactive.Get( Name );
            SoundEffect = JobBars.Configuration.GaugeSoundEffect_2.Get( Name );
            CompletionSoundEffect = JobBars.Configuration.GaugeCompletionSoundEffect_2.Get( Name );
            SetType( JobBars.Configuration.GaugeType.Get( Name, type ) );
        }

        public abstract GaugeTracker GetTracker( int idx );

        private void SetType( GaugeVisualType type ) {
            var validTypes = GetValidGaugeTypes();
            Type = validTypes.Contains( type ) ? type : validTypes[0];
            TypeConfig = Type switch {
                GaugeVisualType.Bar => new GaugeBarConfig( Name ),
                GaugeVisualType.Diamond => new GaugeDiamondConfig( Name ),
                GaugeVisualType.Arrow => new GaugeArrowConfig( Name ),
                GaugeVisualType.BarDiamondCombo => new GaugeBarDiamondComboConfig( Name ),
                _ => null
            };
        }

        public void Draw( string id, out bool newVisual, out bool reset ) {
            newVisual = reset = false;

            if( JobBars.Configuration.GaugeEnabled.Draw( $"启用{id}", Name, Enabled, out var newEnabled ) ) {
                Enabled = newEnabled;
                newVisual = true;
            }

            if( JobBars.Configuration.GaugeHideInactive.Draw( $"非激活时隐藏{id}", Name, HideWhenInactive, out var newHideWhenInactive ) ) {
                HideWhenInactive = newHideWhenInactive;
            }

            if( JobBars.Configuration.GaugeIndividualScale.Draw( $"缩放{id}", Name, out var newScale ) ) {
                Scale = Math.Max( 0.1f, newScale );
                newVisual = true;
            }

            if( JobBars.Configuration.GaugePositionType == GaugePositionType.Split ) {
                if( JobBars.Configuration.GaugeSplitPosition.Draw( $"分离位置{id}", Name, out var newPosition ) ) {
                    SetSplitPosition( newPosition );
                    newVisual = true;
                }
            }

            if( JobBars.Configuration.GaugeOrder.Draw( $"顺序{id}", Name, Order, out var newOrder ) ) {
                Order = newOrder;
                newVisual = true;
            }

            var validTypes = GetValidGaugeTypes();
            if( validTypes.Length > 1 ) {
                if( JobBars.Configuration.GaugeType.Draw( $"Type{id}", Name, validTypes, Type, out var newType ) ) {
                    SetType( newType );
                    reset = true;
                }
            }

            TypeConfig.Draw( id, ref newVisual, ref reset );

            DrawConfig( id, ref newVisual, ref reset );
        }

        protected void DrawSoundEffect( string label = "进度音效" ) {
            if( ImGui.Button( "测试##SoundEffect" ) ) Helper.UiHelper.PlaySoundEffect( SoundEffect );
            ImGui.SameLine();

            ImGui.SetNextItemWidth( 200f );
            if( JobBars.Configuration.GaugeSoundEffect_2.Draw( $"{label} (0 = 关闭)", Name, SoundEffect, out var newSoundEffect ) ) {
                SoundEffect = newSoundEffect;
            }
            ImGui.SameLine();
            HelpMarker( "对于宏音效，需要加36。例如，<se.6> 应该是 6+36=42" );
        }

        public void PlaySoundEffect() => Helper.UiHelper.PlaySoundEffect( SoundEffect );

        protected void DrawCompletionSoundEffect() {
            if( ImGui.Button( "测试##CompletionSoundEffect" ) ) Helper.UiHelper.PlaySoundEffect( CompletionSoundEffect );
            ImGui.SameLine();

            ImGui.SetNextItemWidth( 200f );
            if( JobBars.Configuration.GaugeCompletionSoundEffect_2.Draw( $"完成音效 (0 = 关闭)", Name, CompletionSoundEffect, out var newSoundEffect ) ) {
                CompletionSoundEffect = newSoundEffect;
            }
            ImGui.SameLine();
            HelpMarker( "对于宏音效，需要加36。例如，<se.6> 应该是 6+36=42" );
        }

        public void PlayCompletionSoundEffect() => Helper.UiHelper.PlaySoundEffect( CompletionSoundEffect );

        public static void HelpMarker( string text ) {
            ImGui.SetCursorPosX( ImGui.GetCursorPosX() - 5 );
            ImGui.TextDisabled( "(?)" );
            if( ImGui.IsItemHovered() ) {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos( ImGui.GetFontSize() * 35.0f );
                ImGui.TextUnformatted( text );
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        public void DrawPositionBox() {
            if( JobBars.DrawPositionView( Name + "##GaugePosition", Position, out var pos ) ) {
                JobBars.Configuration.GaugeSplitPosition.Set( Name, pos );
                SetSplitPosition( pos );
                //JobBars.GaugeManager.UpdatePositionScale();
            }
        }

        protected abstract GaugeVisualType[] GetValidGaugeTypes();

        protected abstract void DrawConfig( string id, ref bool newVisual, ref bool reset );

        private void SetSplitPosition( Vector2 pos ) {
            JobBars.SetWindowPosition( Name + "##GaugePosition", pos );
        }
    }
}
