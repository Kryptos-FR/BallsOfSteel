using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallsOfSteel.Core
{
    public static class ScriptComponentExtensions
    {
        public static void SpawnInstance(this ScriptComponent script, Prefab source, Entity attachEntity, float timeout, Matrix localMatrix)
        {
            if (source == null)
                return;

            Func<Task> spawnTask = async () =>
            {
                // Clone
                var spawnedEntities = source.Instantiate();

                // Add
                foreach (var prefabEntity in spawnedEntities)
                {
                    prefabEntity.Transform.UpdateLocalMatrix();
                    var entityMatrix = prefabEntity.Transform.LocalMatrix * localMatrix;
                    entityMatrix.Decompose(out prefabEntity.Transform.Scale, out prefabEntity.Transform.Rotation, out prefabEntity.Transform.Position);

                    if (attachEntity != null)
                    {
                        attachEntity.AddChild(prefabEntity);
                    }
                    else
                    {
                        script.SceneSystem.SceneInstance.RootScene.Entities.Add(prefabEntity);
                    }
                }

                // Countdown
                var secondsCountdown = timeout;
                while (secondsCountdown > 0f)
                {
                    await script.Script.NextFrame();
                    secondsCountdown -= (float)script.Game.UpdateTime.Elapsed.TotalSeconds;
                }

                // Remove
                foreach (var clonedEntity in spawnedEntities)
                {
                    if (attachEntity != null)
                    {
                        attachEntity.RemoveChild(clonedEntity);
                    }
                    else
                    {
                        script.SceneSystem.SceneInstance.RootScene.Entities.Remove(clonedEntity);
                    }
                }

                // Cleanup
                spawnedEntities.Clear();
            };

            script.Script.AddTask(spawnTask);
        }

    }
}
