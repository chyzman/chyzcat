using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
namespace Chyz;

sealed class Chyz : Weapon, IPlayerEdible, IDrawable {
    public float darkness;
    public float lastDarkness;
    public int bites = 4;
    public int variant = Random.Range(0, 3);

    public override bool HeavyWeapon => true;
    public int FoodPoints => 1;
    public bool Edible => true;
    public bool AutomaticPickUp => false;
    public int BitesLeft => bites;

    public AbstractChyz Abstr { get; }

    public Chyz(AbstractChyz abstr, World world) : base(abstr, world) {
        Abstr = abstr;
        bodyChunks = new BodyChunk[1];
        bodyChunks = new[] { new BodyChunk(this, 0, abstractPhysicalObject.Room.realizedRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile), 10f, 0.3f) { goThroughFloors = false } };
        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.2f;
        surfaceFriction = 0.7f;
        collisionLayer = 1;
        waterFriction = 0.95f;
        buoyancy = 1.1f;
        firstChunk.loudness = 9f;
        tailPos = firstChunk.pos;
        soundLoop = new ChunkDynamicSoundLoop(firstChunk);
        color = new Color(255, 255, 0);
    }

    public override void Update(bool eu) {
        base.Update(eu);
        soundLoop.sound = SoundID.None;
        if (firstChunk.vel.magnitude > 5f) {
            if (firstChunk.ContactPoint.y < 0) {
                soundLoop.sound = SoundID.Rock_Skidding_On_Ground_LOOP;
            }
            else {
                soundLoop.sound = SoundID.Rock_Through_Air_LOOP;
            }
            soundLoop.Volume = Mathf.InverseLerp(5f, 15f, firstChunk.vel.magnitude);
        }
        soundLoop.Update();
        if (firstChunk.ContactPoint.y != 0) {
            rotationSpeed = (rotationSpeed * 2f + firstChunk.vel.x * 5f) / 3f;
        }
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu) {
        base.HitSomething(result, eu);
        if (result.obj == null) {
            return false;
        }
        if (thrownBy is Scavenger && (thrownBy as Scavenger).AI != null) {
            (thrownBy as Scavenger).AI.HitAnObjectWithWeapon(this, result.obj);
        }
        vibrate = 20;
        if (result.obj is Creature) {
            float stunBonus = 45f;
            if (ModManager.MMF && MMF.cfgIncreaseStuns.Value && (result.obj is Cicada || result.obj is LanternMouse || (ModManager.MSC && result.obj is Yeek))) {
                stunBonus = 90f;
            }
            if (ModManager.MSC && room.game.IsArenaSession && room.game.GetArenaGameSession.chMeta != null) {
                stunBonus = 90f;
            }
            (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, 0.01f, stunBonus);
        }
        else if (result.chunk != null) {
            result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
        }
        else if (result.onAppendagePos != null) {
            (result.obj as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
        }
        firstChunk.vel = firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * firstChunk.vel.magnitude;
        room.PlaySound(SoundID.Rock_Hit_Creature, firstChunk);
        if (result.chunk != null) {
            room.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
        }
        SetRandomSpin();
        return true;
    }

    public override void HitByWeapon(Weapon weapon) {
    }

    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu) {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        if (room == null) return;
        room.PlaySound(SoundID.Slugcat_Throw_Rock, firstChunk);
    }

    public override void PickedUp(Creature upPicker) {
        base.PickedUp(upPicker);
        room.PlaySound(SoundID.Slugcat_Pick_Up_Rock, firstChunk);
    }

    public void BitByPlayer(Creature.Grasp grasp, bool eu) {
        bites--;
        room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, firstChunk.pos);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (bites < 1) {
            (grasp.grabber as Player).ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public void ThrowByPlayer() { }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
        sLeaser.sprites = new FSprite[4];
        sLeaser.sprites[0] = new FSprite($"ChyzObj{variant}-0");
        sLeaser.sprites[1] = new FSprite($"ChyzObj{variant}-1");
        sLeaser.sprites[2] = new FSprite($"ChyzObj{variant}-2");
        sLeaser.sprites[3] = new FSprite($"ChyzObj{variant}-3");
        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
        Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
        sLeaser.sprites[0].rotation = Custom.VecToDeg(v);
        sLeaser.sprites[0].x = vector.x - camPos.x;
        sLeaser.sprites[0].y = vector.y - camPos.y;
        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName($"ChyzObj{variant}-" + Custom.IntClamp(4 - bites, 0, 3));
        sLeaser.sprites[0].scale = 1f;
        if (blink > 0 && Random.value < 0.5f) {
            sLeaser.sprites[0].color = blinkColor;
        }
        else {
            sLeaser.sprites[0].color = color;
        }
        if (slatedForDeletetion || room != rCam.room) {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
        if (newContatiner == null) {
            newContatiner = rCam.ReturnFContainer("Items");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++) {
            sLeaser.sprites[i].RemoveFromContainer();
        }
        newContatiner.AddChild(sLeaser.sprites[0]);
        newContatiner.AddChild(sLeaser.sprites[1]);
        newContatiner.AddChild(sLeaser.sprites[2]);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
        for (int i = 0; i < sLeaser.sprites.Length; i++) {
            color = new Color(255, 255, 0);
        }
    }
}