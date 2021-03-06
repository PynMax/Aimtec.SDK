﻿namespace Aimtec.SDK.Prediction.Health
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    using Aimtec.SDK.Extensions;

    class HealthPredictionImpl : IHealthPrediction
    {
        class Attack
        {
            public Obj_AI_Base Source { get; set; }
            public Obj_AI_Base Target { get; set; }
            public float MissileSpeed { get; set; }

            public bool Active { get; set; } = true;
        }

        #region Constructors and Destructors

        private Dictionary<int, HashSet<Attack>> Attacks { get; } = new Dictionary<int, HashSet<Attack>>();

        public HealthPredictionImpl()
        {
            Game.OnUpdate += this.OnUpdate;
            Obj_AI_Base.OnProcessAutoAttack += this.OnProcessAutoAttack;
            SpellBook.OnStopCast += this.OnStopCast;
        }

        private void OnUpdate()
        {
            // Invalidate attacks whose targets are not valid targets
            foreach (var value in this.Attacks.Values)
            {
                value.RemoveWhere(x => !x.Target.IsValidTarget());
            }
        }

        private void OnStopCast(Obj_AI_Base sender, SpellBookStopCastEventArgs e)
        {
            if (!e.DestroyMissile || !e.StopAnimation || (!(sender is Obj_AI_Minion)
                || sender.Type == GameObjectType.obj_AI_Turret))
            {
                return;
            }

            HashSet<Attack> value;
            if (!this.Attacks.TryGetValue(sender.NetworkId, out value))
            {
                return;
            }

            value.RemoveWhere(x => x.Active);
        }

        private void OnProcessAutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs e)
        {
            var target = sender as Obj_AI_Minion;

            // TODO: ENHANCEMENT: Account for turret attacks
            if (target == null || target.Type == GameObjectType.obj_AI_Turret)
            {
                return;
            }

            HashSet<Attack> value;
            if (!this.Attacks.TryGetValue(target.NetworkId, out value))
            {
                value = new HashSet<Attack>();
                this.Attacks[target.NetworkId] = value;
            }

            // TODO: Add damage to this (Missing in Spell data API)
            value.Add(new Attack { Source = sender, Target = e.Sender,});

        }

        #endregion

        #region Public Methods and Operators

        public float GetPrediction(Obj_AI_Base target, int time)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        public static float CalculateMissileTravelTime(
            Obj_AI_Base target,
            Vector2 sourcePosition,
            float missileDelay,
            float missileSpeed)
        {
            if (missileSpeed <= 0 || missileSpeed >= float.MaxValue)
            {
                return 0f;
            }

            var minionPosition = target.Position;

            if (!target.HasPath || target.Path.Length == 2)
            {
                return sourcePosition.Distance(minionPosition.To2D()) / missileSpeed;
            }

            var pathEndPosition = target.Path.LastOrDefault();
            var direction = (pathEndPosition - minionPosition).Normalized();
            var totalPathLength = pathEndPosition.Distance(minionPosition);
            var delayDistance = missileDelay * target.MoveSpeed;

            if (delayDistance <= totalPathLength)
            {
                var positionAfterDelay = minionPosition + direction * delayDistance;
                var angle = GetAngle(Vector2.Normalize((Vector2)positionAfterDelay - sourcePosition), (Vector2) direction);
                var c = Math.Sin(target.MoveSpeed / missileSpeed * Math.Sin(angle));

                if (Math.PI - angle - Math.Asin(c) >= 0)
                {
                    var playerDistance =
                        (float)
                        (c / Math.Sin(Math.PI - angle - Math.Asin(c))
                            * positionAfterDelay.To2D().Distance(sourcePosition));
                    var predictedPos = positionAfterDelay + direction * playerDistance;

                    minionPosition = playerDistance + delayDistance <= totalPathLength ? predictedPos : pathEndPosition;
                }
                else
                {
                    minionPosition = pathEndPosition;
                }
            }
            else
            {
                minionPosition = pathEndPosition;
            }

            return sourcePosition.Distance(minionPosition.To2D()) / missileSpeed;
        }

        private static float GetAngle(Vector2 v1, Vector2 v2)
        {
            return
                (float)
                Math.Acos(
                    ((v1.X * v2.X) + (v1.Y * v2.Y))
                    / ((Math.Sqrt(v1.X.Pow() + v1.Y.Pow())) * (Math.Sqrt(v2.X.Pow() + v2.Y.Pow()))));
        }

        #endregion
    }
}