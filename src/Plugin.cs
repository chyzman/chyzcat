using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using Fisobs.Core;
using IL.MoreSlugcats;
using ImprovedInput;
using JetBrains.Annotations;
using SlugBase;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using LillyPuck = MoreSlugcats.LillyPuck;


namespace Chyz {
    [BepInPlugin(MOD_ID, "Chyz Scug", "0.1.0")]
    class Plugin : BaseUnityPlugin {
        private const string MOD_ID = "chyzman.chyz";
        public static Plugin Instance;
        public static ChyzOption Option = new ChyzOption();

        [CanBeNull] private static SlugcatStats.Name _chyzName;

        public static ConditionalWeakTable<Player, WallClimb> wallClimb = new();

        public static ConditionalWeakTable<Player, PhysicalObject> previousGrasp = new();

        public static ConditionalWeakTable<PhysicalObject, ChyzTouched> chyzTouched = new();

        public static readonly PlayerKeybind ChyzModifierKey = PlayerKeybind.Register("chyz:modifier", "The Chyz", "Chyz Modifier", KeyCode.LeftAlt, KeyCode.JoystickButton3);

        public static SlugcatStats.Name chyzName {
            get {
                if (_chyzName == null) _chyzName = SlugBaseCharacter.Registry.Keys.First(name => name.value == "Chyz");
                return _chyzName;
            }
        }

        public bool isChyz(Player player) {
            return player.slugcatStats.name == chyzName;
        }

        public bool IsModifierPressed(Player player) {
            return ChyzModifierKey.CheckRawPressed(player.playerState.playerNumber);
        }

        public void OnEnable() {
            Content.Register(new ChyzFisob());
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.Player.ctor += (orig, self, creature, world) => {
                orig(self, creature, world);
                if (isChyz(self)) {
                    wallClimb.Add(self, new WallClimb(self));
                    previousGrasp.Add(self, null);
                }
            };
            On.Player.Jump += (orig, self) => {
                orig(self);
                if (!isChyz(self)) return;
                self.jumpBoost *= 1.5f;
            };
            On.Player.Die += Player_Die;
            On.Player.ThrowObject += (orig, self, grasp, eu) => {
                PhysicalObject physicalObject = self.grasps[grasp].grabbed;
                if (isChyz(self)) {
                    previousGrasp.Remove(self);
                    previousGrasp.Add(self, physicalObject);
                }

                if (isChyz(self) && IsModifierPressed(self) && !(physicalObject is Spear)) {
                    var vel = self.firstChunk.vel;
                    if (vel.magnitude > 5f) {
                        vel = self.firstChunk.vel * 10f;
                    }
                    else {
                        vel = new Vector2(self.input[0].x, self.input[0].y) * 25f;
                    }
                    self.grasps[grasp].Release();
                    foreach (var chunk in physicalObject.bodyChunks) {
                        chunk.vel = vel;
                    }
                }
                else {
                    orig(self, grasp, eu);
                }
            };
            On.Player.Update += (orig, self, eu) => {
                orig(self, eu);
                if (!isChyz(self)) return;
                if (self.grasps != null) {
                    foreach (var grasp in self.grasps) {
                        if (grasp != null) {
                            ChyzTouched.Touch(grasp.grabbed, self);
                        }
                    }
                }
                if (self.spearOnBack.HasASpear) {
                    ChyzTouched.Touch(self.spearOnBack.spear, self);
                }

                if (wallClimb.TryGetValue(self, out WallClimb wallclimb)) {
                    wallclimb.Update();
                }

                if (self.firstChunk.pos.y < 0) {
                    self.firstChunk.vel += new Vector2(0, Math.Max(20, -self.firstChunk.pos.y));
                }
            };
            On.Player.Collide += (orig, self, otherObject, chunk, otherChunk) => {
                orig(self, otherObject, chunk, otherChunk);
                if (!isChyz(self)) return;
                if (otherObject != null) {
                    ChyzTouched.Touch(otherObject, self);
                }
            };
            On.Player.UpdateBodyMode += (orig, self) => {
                orig(self);
                if (!isChyz(self)) return;
                if (self.bodyMode == Player.BodyModeIndex.Crawl) {
                    self.dynamicRunSpeed[0] *= 3;
                    self.dynamicRunSpeed[1] *= 3;
                }
                else {
                    self.dynamicRunSpeed[0] *= 1f;
                    self.dynamicRunSpeed[1] *= 1f;
                }

                if (self.bodyMode == Player.BodyModeIndex.Default) {
                    self.dynamicRunSpeed[0] *= 1.5f;
                    self.dynamicRunSpeed[1] *= 1.5f;
                }

                if (self.bodyMode == Player.BodyModeIndex.Stand) {
                    self.dynamicRunSpeed[0] *= 0.7f;
                    self.dynamicRunSpeed[1] *= 0.7f;
                }
            };
            On.PhysicalObject.Update += (orig, self, eu) => {
                orig(self, eu);
                if (!chyzTouched.TryGetValue(self, out ChyzTouched touched)) return;
                touched.UpdateColor();
            };
            On.Player.PickupPressed += (orig, self) => {
                if (isChyz(self) && IsModifierPressed(self)) {
                    if (previousGrasp.TryGetValue(self, out PhysicalObject physicalObject)) {
                        if (physicalObject != null && physicalObject.room == self.room) {
                            for (int i = 0; i < self.grasps.Length; i++) {
                                if (self.grasps[i] == null) {
                                    foreach (var chunk in physicalObject.bodyChunks) {
                                        chunk.pos = self.firstChunk.pos;
                                        chunk.vel = self.firstChunk.vel;
                                    }

                                    self.Grab(physicalObject, i, 0, Creature.Grasp.Shareability.NonExclusive, 100000, true, true);
                                }
                            }
                        }
                    }
                }
                else {
                    orig(self);
                }
            };
            On.PlayerGraphics.InitiateSprites += (orig, self, sleaser, rcam) => {
                orig(self, sleaser, rcam);
                if (isChyz(self.player)) {
                    sleaser.sprites[0] = new FSprite("ChyzBody");
                    sleaser.sprites[1] = new FSprite("ChyzHips");
                    sleaser.sprites[3] = new FSprite("ChyzHeadA0");
                }
                // foreach (var sprite in sleaser.sprites) {
                //     Debug.Log(string.Format($"<Chyz> : {sprite._element.name}"));
                // }
                self.AddToContainer(sleaser, rcam, null);
            };
            On.PlayerGraphics.DrawSprites += (orig, self, leaser, cam, stacker, pos) => {
                orig(self, leaser, cam, stacker, pos);
                // if (isChyz(self.player)) {
                //     var head = leaser.sprites[3];
                //     if (!self.RenderAsPup) {
                //         if (!head.element.name.Contains("Chyz") && head.element.name.StartsWith("HeadA")) head.SetElementByName("Chyz" + head.element.name);
                //     }
                // }
            };
            ChyzEnum.RegisterValues();
        }

        private void LoadResources(RainWorld rainWorld) {
            Futile.atlasManager.LoadAtlas("atlases/ChyzObj");
            Futile.atlasManager.LoadAtlas("atlases/ChyzAll");
            // Futile.atlasManager.LogAllElementNames();
            try {
                MachineConnector.SetRegisteredOI(MOD_ID, Option);
            }
            catch (Exception ex) {
                Debug.Log(string.Format($"{MOD_ID} options failed init error {Option}{ex}"));
            }

            Option.Initialize();
        }

        private void Player_Die(On.Player.orig_Die orig, Player player) {
            orig(player);
            if (!isChyz(player)) return;
            var room = player.room;
            var pos = player.mainBodyChunk.pos;
            var color = player.ShortCutColor();
            room.AddObject(new Explosion(room, player, pos, 7, 250f, 69f, 5f, 280f, 0.25f, player, 0.7f, 160f, 1f));
            room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, color));
            room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            room.AddObject(new ExplosionSpikes(room, pos, 14, 30f, 9f, 10f, 170f, color));
            room.AddObject(new ShockWave(pos, 330f, 4.5f, 5, false));

            room.ScreenMovement(pos, default, 1.3f);
            room.PlaySound(SoundID.Bomb_Explode, pos);
            room.InGameNoise(new Noise.InGameNoise(pos, 9000f, player, 1f));
        }
    }
}