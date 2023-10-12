using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Chyz;

public class WallClimb : MonoBehaviour {
    public Player self;

    public int climbDuration = 0;

    public bool IsBackClimbing;

    public WallClimb(Player player) {
        self = player;
        IsBackClimbing = true;
    }

    public bool CanCeilingClimb(Player player, Vector2 pos, Vector2 range) {
        return (player.room.aimap.getAItile(pos + 20f * range.normalized).acc == AItile.Accessibility.Solid ||
                player.room.aimap.getAItile(pos + 20f * range.normalized).acc == AItile.Accessibility.Corridor) &&
                player.room.aimap.getAItile(pos + 30f * range.normalized).acc != null &&
                player.room.aimap.getAItile(pos - 50f * range.normalized).acc != AItile.Accessibility.Solid &&
                player.room.aimap.getAItile(pos - 30f * range.normalized).acc != AItile.Accessibility.Solid &&
                self.bodyMode != Player.BodyModeIndex.CorridorClimb &&
                self.animation != Player.AnimationIndex.CorridorTurn &&
                self.bodyMode != Player.BodyModeIndex.Swimming && self.bodyMode != Player.BodyModeIndex.ZeroG;
    }

    public bool BackGroundWallCheck(Player player, Vector2 pos) {
        return player.room.aimap.getAItile(pos).acc == AItile.Accessibility.Wall ||
                player.room.aimap.getAItile(pos).acc == AItile.Accessibility.Climb ||
                player.room.aimap.getAItile(pos).acc == AItile.Accessibility.Floor;
    }

    public bool CanZeroGClimb(Player player, Vector2 pos, float range) {
        return !(self.bodyMode != Player.BodyModeIndex.ZeroG) &&
                (player.room.aimap.getAItile(pos + range * Vector2.right).acc == AItile.Accessibility.Solid ||
                player.room.aimap.getAItile(pos + range * Vector2.left).acc == AItile.Accessibility.Solid ||
                player.room.aimap.getAItile(pos + range * Vector2.up).acc == AItile.Accessibility.Solid ||
                player.room.aimap.getAItile(pos + range * Vector2.down).acc == AItile.Accessibility.Solid ||
                player.room.aimap.getAItile(pos + 1.75f * range * new Vector2(1f, 1f)).acc ==
                AItile.Accessibility.Solid ||
                player.room.aimap.getAItile(pos + 1.75f * range * new Vector2(-1f, 1f)).acc ==
                AItile.Accessibility.Solid ||
                player.room.aimap.getAItile(pos + 1.75f * range * new Vector2(1f, -1f)).acc ==
                AItile.Accessibility.Solid ||
                player.room.aimap.getAItile(pos + 1.75f * range * new Vector2(-1f, -1f)).acc ==
                AItile.Accessibility.Solid);
    }

    public void Update() {
        if (self.room != null) {
            if (self.bodyMode == Player.BodyModeIndex.WallClimb) {
                self.customPlayerGravity = 0.01f;
                self.mainBodyChunk.lastPos = self.mainBodyChunk.pos;
                self.mainBodyChunk.vel.y = 0f;
                self.animation = Player.AnimationIndex.DownOnFours;
                if (climbDuration < 20) {
                    if (self.input[0].y > 0) {
                        climbDuration++;
                        self.mainBodyChunk.vel.y = Mathf.Lerp(10 * self.input[0].y, 0f, 0.075f * climbDuration);
                    }
                    else {
                        climbDuration = Mathf.Clamp(climbDuration - 1, 0, 10);
                        self.mainBodyChunk.vel.y = 5 * self.input[0].y;
                    }
                }
                else {
                    climbDuration = 0;
                    self.mainBodyChunk.vel.y = 0f;
                }
            }
            else {
                self.customPlayerGravity = self.room.gravity;
            }

            if (CanCeilingClimb(self, self.mainBodyChunk.pos, Vector2.up) && self.input[0].y >= 0) {
                self.bodyMode = ChyzEnum.CeilingClimb;
                self.customPlayerGravity = 0.001f;
                self.mainBodyChunk.vel.y = 5f;
                if (self.mainBodyChunk.vel.x <= 4f && self.mainBodyChunk.vel.x >= -4f) {
                    BodyChunk mainBodyChunk = self.mainBodyChunk;
                    mainBodyChunk.vel.x += self.input[0].x;
                }
                else {
                    BodyChunk mainBodyChunk2 = self.mainBodyChunk;
                    mainBodyChunk2.vel.x *= 0.75f;
                }

                (self.graphicsModule as PlayerGraphics).LookAtPoint(self.mainBodyChunk.pos + self.mainBodyChunk.vel,
                    10f);
            }

            if (self.feetStuckPos != null || (IsBackClimbing && self.input[0].jmp) ||
                self.bodyMode == Player.BodyModeIndex.Stand) {
                IsBackClimbing = false;
            }

            // if (BackGroundWallCheck(self, self.mainBodyChunk.pos) && Plugin.Instance.IsModifierPressed(self)) {
            //     IsBackClimbing = true;
            // }

            if (IsBackClimbing) {
                if (BackGroundWallCheck(self, self.mainBodyChunk.pos)) {
                    self.bodyMode = ChyzEnum.BackWallClimb;
                }
            }

            if (self.bodyMode == ChyzEnum.BackWallClimb) {
                self.animation = Player.AnimationIndex.None;
                self.noGrabCounter = 10;
                self.customPlayerGravity = 0.001f;
                Vector2 moveDir = new Vector2(self.input[0].x, self.input[0].y);
                Vector2 rotateDir = Custom.DirVec(self.mainBodyChunk.pos - self.bodyChunks[1].pos, moveDir);
                if (self.mainBodyChunk.vel.magnitude <= (self.input[0].y <= 0 ? 5f : 8f)) {
                    if (self.input[0].x != 0 || self.input[0].y != 0) {
                        BodyChunk mainBodyChunk3 = self.mainBodyChunk;
                        mainBodyChunk3.vel.x += 10f * moveDir.x;
                        BodyChunk mainBodyChunk4 = self.mainBodyChunk;
                        mainBodyChunk4.vel.y += 10f * moveDir.y;
                    }
                }

                BodyChunk mainBodyChunk5 = self.mainBodyChunk;
                mainBodyChunk5.vel.x *= 0.5f;
                BodyChunk mainBodyChunk6 = self.mainBodyChunk;
                mainBodyChunk6.vel.y *= 0.5f;
                self.bodyChunks[1].vel *= Vector2.zero;
                if (self.input[0].x == 0 && self.input[0].y == 0) {
                    self.bodyChunks[0].vel *= moveDir.normalized;
                }
                else {
                    if (self.input[0].x != 0 && self.input[0].y == 0 &&
                        Vector2.SignedAngle(Vector2.up, self.mainBodyChunk.pos - self.bodyChunks[1].pos) < 90f) {
                        self.mainBodyChunk.vel += 8f * moveDir;
                        self.mainBodyChunk.vel.y = 0f;
                    }
                }

                (self.graphicsModule as PlayerGraphics).LookAtPoint(
                    self.mainBodyChunk.pos + 40f * (self.bodyChunks[0].pos - self.bodyChunks[1].pos).normalized, 100f);
            }
        }
    }
}