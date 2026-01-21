using JobBars.Atk;
using JobBars.Buffs;
using JobBars.Cooldowns;
using JobBars.Cursors;
using JobBars.Data;
using JobBars.Gauges;
using JobBars.Gauges.Stacks;
using JobBars.Gauges.Timer;
using JobBars.Helper;
using JobBars.Icons;

namespace JobBars.Jobs
{
    public static class WAR
    {
        public static GaugeConfig[] Gauges =>
        [
            new GaugeStacksConfig( UiHelper.Localize( BuffIds.InnerRelease ), GaugeVisualType.Diamond,
                new GaugeStacksProps
                {
                    MaxStacks = 3,
                    Triggers =
                    [
                        new Item( BuffIds.InnerRelease )
                    ],
                    Color = ColorConstants.Orange
                } ),
            new GaugeTimerConfig( UiHelper.Localize( BuffIds.SurgingTempest ), GaugeVisualType.Bar,
                new GaugeSubTimerProps
                {
                    MaxDuration = 60,
                    DefaultDuration = 30,
                    Color = ColorConstants.Red,
                    Triggers =
                    [
                        new Item( BuffIds.SurgingTempest )
                    ]
                } )
        ];

        public static BuffConfig[] Buffs =>
        [
            new BuffConfig( UiHelper.Localize( BuffIds.InnerRelease ), new BuffProps
            {
                Duration = 15,
                CD = 60,
                Icon = ActionIds.InnerRelease,
                Color = ColorConstants.Orange,
                Triggers = [new Item( BuffIds.InnerRelease )]
            } )
        ];

        public static Cursor Cursors => new(JobIds.WAR, CursorType.None, CursorType.GCD);

        public static CooldownConfig[] Cooldowns =>
        [
            new CooldownConfig( UiHelper.Localize( ActionIds.Bloodwhetting ), new CooldownProps
            {
                Icon = ActionIds.Bloodwhetting,
                // Duration = 6,
                CD = 25,
                Triggers =
                [
                    new Item( ActionIds.NascentFlash ),
                    new Item( ActionIds.RawIntuition ),
                    new Item( ActionIds.Bloodwhetting )
                ]
            } ),
            new CooldownConfig( UiHelper.Localize( ActionIds.铁壁 ), new CooldownProps
            {
                Icon = ActionIds.铁壁,
                // Duration = 20,
                CD = 90,
                Triggers = [new Item( ActionIds.铁壁)]
            } ),
            new CooldownConfig( UiHelper.Localize( ActionIds.ThrillOfBattle ), new CooldownProps
            {
                Icon = ActionIds.ThrillOfBattle,
                // Duration = 10,
                CD = 90,
                Triggers = [new Item( ActionIds.ThrillOfBattle )]
            } ),
            new CooldownConfig( UiHelper.Localize( ActionIds.复仇), new CooldownProps
            {
                Icon = ActionIds.复仇,
                // Duration = 15,
                CD = 120,
                Triggers = [new Item( ActionIds.复仇),new Item(ActionIds.Damnation)]
            } ),
            new CooldownConfig( UiHelper.Localize( ActionIds.Holmgang ), new CooldownProps
            {
                Icon = ActionIds.Holmgang,
                // Duration = 10,
                CD = 240,
                Triggers = [new Item( ActionIds.Holmgang )]
            } ),
            new CooldownConfig( $"{UiHelper.Localize( ActionIds.雪仇 )} ({UiHelper.Localize( JobIds.WAR )})",
                new CooldownProps
                {
                    Icon = ActionIds.雪仇,
                    Duration = 15,
                    CD = 60,
                    Triggers = [new Item( ActionIds.雪仇 )]
                } ),
            new CooldownConfig( UiHelper.Localize( ActionIds.ShakeItOff ), new CooldownProps
            {
                Icon = ActionIds.ShakeItOff,
                // Duration = 15,
                CD = 90,
                Triggers = [new Item( ActionIds.ShakeItOff )]
            } ),
        ];

        public static IconReplacer[] Icons => new[]
        {
            new IconBuffReplacer( UiHelper.Localize( BuffIds.SurgingTempest ), new IconBuffProps
            {
                IconType = IconActionType.Timer,
                Icons = [ActionIds.StormsEye],
                Triggers =
                [
                    new IconBuffTriggerStruct { Trigger = new Item( BuffIds.SurgingTempest ), Duration = 60 }
                ]
            } ),
            new IconBuffReplacer( $"{UiHelper.Localize( ActionIds.铁壁 )} ({UiHelper.Localize( JobIds.WAR )})",
                new IconBuffProps
                {
                    IconType = IconActionType.Buff,
                    Icons = [ActionIds.铁壁],
                    Triggers =
                    [
                        new IconBuffTriggerStruct { Trigger = new Item( BuffIds.铁壁 ), Duration = 20 }
                    ]
                } ),
            new IconBuffReplacer( UiHelper.Localize( BuffIds.Damnation ), new IconBuffProps
            {
                IconType = IconActionType.Buff,
                Icons =
                [
                    ActionIds.复仇,
                    ActionIds.Damnation
                ],
                Triggers =
                [
                    new IconBuffTriggerStruct { Trigger = new Item( BuffIds.Vengeance ), Duration = 15 },
                    new IconBuffTriggerStruct { Trigger = new Item( BuffIds.Damnation ), Duration = 15 }
                ]
            } ),
            new IconBuffReplacer( UiHelper.Localize( BuffIds.Bloodwhetting ), new IconBuffProps
            {
                IconType = IconActionType.Buff,
                Icons =
                [
                    ActionIds.RawIntuition,
                    ActionIds.Bloodwhetting
                ],
                Triggers =
                [
                    new IconBuffTriggerStruct { Trigger = new Item( BuffIds.RawIntuition ), Duration = 6 },
                    new IconBuffTriggerStruct { Trigger = new Item( BuffIds.Bloodwhetting ), Duration = 8 }
                ]
            } ),
            new IconBuffReplacer( $"{UiHelper.Localize( ActionIds.ArmsLength )} ({UiHelper.Localize( JobIds.WAR )})",
                new IconBuffProps
                {
                    IconType = IconActionType.Buff,
                    Icons = [ActionIds.ArmsLength],
                    Triggers =
                    [
                        new IconBuffTriggerStruct { Trigger = new Item( BuffIds.ArmsLength ), Duration = 6 }
                    ]
                } ),
            new IconBuffReplacer( UiHelper.Localize( BuffIds.Holmgang ), new IconBuffProps
            {
                IconType = IconActionType.Buff,
                Icons = [ActionIds.Holmgang],
                Triggers =
                [
                    new IconBuffTriggerStruct { Trigger = new Item( BuffIds.Holmgang ), Duration = 10 }
                ]
            } ),
            new IconBuffReplacer( UiHelper.Localize( BuffIds.ThrillOfBattle ), new IconBuffProps
            {
                IconType = IconActionType.Buff,
                Icons = [ActionIds.ThrillOfBattle],
                Triggers =
                [
                    new IconBuffTriggerStruct { Trigger = new Item( BuffIds.ThrillOfBattle ), Duration = 10 }
                ]
            } )
        };

        public static bool MP => false;

        public static float[] MP_SEGMENTS => null;
    }
}