// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BallsOfSteel.Core;
using BallsOfSteel.Player;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace Gamelogic
{
    public class MainGameLogic : SyncScript
    {
        private Random rand = new Random();

        // Declared public member fields and properties will show in the game studio
        public List<Material> VariousNations = new List<Material>();


        private Entity[] players = new Entity[8]; // This is totally hardcoded

        public CameraComponent MainCamera { get; set; }

        public Prefab NewPlayerPrefab { get; set; }


        public override void Start()
        {
            // Initialization of the script.
        }

        protected Entity RegisterNewPlayer(Prefab playerPrefab, int controllerId)
        {
            if (playerPrefab == null)
                return null;

            var player = playerPrefab.Instantiate();
            if (player.Count != 1)
                throw new InvalidOperationException("The player prefab must have exactly ONE root entity!");

            var playerInputControl = player[0].Get<PlayerInputControl>();
            playerInputControl.Camera = MainCamera;

            // Assign the proper controller id
            var xboxController = player[0].GetOrCreate<XboxInput>();
            xboxController.ControllerID = controllerId;
            playerInputControl.ControlInput = xboxController;

            // Assign a random nation
            if (VariousNations.Count > 0)
            {
                int idx = rand.Next() % VariousNations.Count;
                var modelComponent = playerInputControl?.ModelChildEntity?.Get<ModelComponent>();
                if (modelComponent != null)
                {
                    modelComponent.Materials[0] = VariousNations[idx];
                }
            }

            SceneSystem.SceneInstance.RootScene.Entities.Add(player[0]);

            playerInputControl.Respawn(new Vector3(0, 2, 0));

            return player[0];
        }

        public override void Update()
        {
            // Check for new players connecting locally
            for (int i = 0; i < 8; i++)
            {
                if (players[i] != null)
                    continue;

                if (Input.IsGamePadButtonDown(GamePadButton.A, i))
                {
                    players[i] = RegisterNewPlayer(NewPlayerPrefab, i);
                }
            }

            // Check for new players connecting remotely

            // Do stuff every new frame

        }
    }
}
