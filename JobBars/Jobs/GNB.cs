using JobBars.Atk;
using JobBars.Buffs;
using JobBars.Cooldowns;
using JobBars.Cursors;
using JobBars.Data;
using JobBars.Gauges;
using JobBars.Gauges.GCD;
using JobBars.Helper;
using JobBars.Icons;

namespace JobBars.Jobs {
    public static class GNB {
        public static GaugeConfig[] Gauges => [
            new GaugeGCDConfig(UiHelper.Localize(BuffIds.NoMercy), GaugeVisualType.Arrow, new GaugeSubGCDProps {
                MaxCounter = 9,
                MaxDuration = 20,
                Color = ColorConstants.Orange,
                Triggers = [
                    new Item(BuffIds.NoMercy)
                ]
            })
        ];

        public static BuffConfig[] Buffs => [];

        public static Cursor Cursors => new( JobIds.GNB, CursorType.None, CursorType.GCD );

        public static CooldownConfig[] Cooldowns => [
            new CooldownConfig(UiHelper.Localize(ActionIds.石之心), new CooldownProps {
                Icon = ActionIds.石之心,
                Duration = 7,
                CD = 25,
                Triggers = [new Item(ActionIds.石之心)]
            }),
            new CooldownConfig(UiHelper.Localize(ActionIds.铁壁), new CooldownProps {
                Icon = ActionIds.铁壁,
                // Duration = 20,
                CD = 90,
                Triggers = [new Item(ActionIds.铁壁)]
            }),
            new CooldownConfig(UiHelper.Localize(ActionIds.伪装), new CooldownProps {
                Icon = ActionIds.伪装,
                // Duration = 20,
                CD = 90,
                Triggers = [new Item(ActionIds.伪装)]
            }),
            new CooldownConfig($"{UiHelper.Localize(ActionIds.星云)} ({UiHelper.Localize(JobIds.GNB)})", new CooldownProps {
                Icon = ActionIds.星云,
                // Duration = 15,
                CD = 120,
                Triggers = [new Item(ActionIds.星云)]
            }),
            new CooldownConfig(UiHelper.Localize(ActionIds.超火流星), new CooldownProps {
                Icon = ActionIds.超火流星,
                // Duration = 10,
                CD = 360,
                Triggers = [new Item(ActionIds.超火流星)]
            }),
            new CooldownConfig($"{UiHelper.Localize(ActionIds.雪仇)} ({UiHelper.Localize(JobIds.GNB)})", new CooldownProps {
                Icon = ActionIds.雪仇,
                // Duration = 15,
                CD = 60,
                Triggers = [new Item(ActionIds.雪仇)]
            }),
            new CooldownConfig(UiHelper.Localize(ActionIds.光之心), new CooldownProps {
                Icon = ActionIds.光之心,
                // Duration = 15,
                CD = 90,
                Triggers = [new Item(ActionIds.光之心)]
            })
        ];

        public static IconReplacer[] Icons => new[] {
            new IconBuffReplacer(UiHelper.Localize(BuffIds.NoMercy), new IconBuffProps {
                IconType = IconActionType.Buff,
                Icons = [
                    ActionIds.NoMercy,
                ],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.NoMercy), Duration = 20 }
                ]
            }),
            new IconBuffReplacer(UiHelper.Localize(BuffIds.ReadyToBreak), new IconBuffProps {
                IconType = IconActionType.GCD,
                Icons = [
                    ActionIds.SonicBreak,
                ],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.ReadyToBreak), Duration = 30 }
                ]
            }),
            new IconBuffReplacer($"{UiHelper.Localize(ActionIds.铁壁)} ({UiHelper.Localize(JobIds.GNB)})", new IconBuffProps {
                IconType = IconActionType.Buff,
                Icons = [ActionIds.铁壁],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.铁壁), Duration = 20 }
                ]
            }),
            new IconBuffReplacer(UiHelper.Localize(BuffIds.GreatNebula), new IconBuffProps {
                IconType = IconActionType.Buff,
                Icons = [
                    ActionIds.星云,
                    ActionIds.GreatNebula
                ],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.Nebula), Duration = 15 },
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.GreatNebula), Duration = 15 }
                ]
            }),
            new IconBuffReplacer(UiHelper.Localize(BuffIds.Camouflage), new IconBuffProps {
                IconType = IconActionType.Buff,
                Icons = [ActionIds.伪装],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.Camouflage), Duration = 20 }
                ]
            }),
            new IconBuffReplacer($"{UiHelper.Localize(ActionIds.ArmsLength)} ({UiHelper.Localize(JobIds.GNB)})", new IconBuffProps {
                IconType = IconActionType.Buff,
                Icons = [ActionIds.ArmsLength],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.ArmsLength), Duration = 6 }
                ]
            }),
            new IconBuffReplacer(UiHelper.Localize(BuffIds.Superbolide), new IconBuffProps {
                IconType = IconActionType.Buff,
                Icons = [ActionIds.超火流星],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.Superbolide), Duration = 10 }
                ]
            }),
            new IconBuffReplacer(UiHelper.Localize(BuffIds.HeartOfCorundum), new IconBuffProps {
                IconType = IconActionType.Buff,
                Icons =
                [
                    ActionIds.石之心,
                    ActionIds.HeartOfCorundum
                ],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.HeartOfStone), Duration = 7 },
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.HeartOfCorundum), Duration = 8 }
                ]
            })
        };

        public static bool MP => false;

        public static float[] MP_SEGMENTS => null;
    }
}
