# Project Overview

This Unity project was developed to create a simple level with two characters — a player and an enemy — in a knight-themed sword-fighting scenario. The level showcases player controls, enemy AI, and combat mechanics, all implemented in Unity.

## Key Features

### 1. Player and Enemy Combat
- **Player Movement**: The player character moves using Unity's new Input System.
- **Camera Control**: The camera follows the player using Cinemachine for smooth, dynamic tracking.
- **Combat Mechanics**: 
  - **Attack**: The player can attack the enemy with a sword by pressing the left mouse button.
  - **Block**: The player can block enemy attacks by holding the right mouse button.
- **Health System**: Both the player and the enemy have health bars that decrease when taking damage. Defeat occurs when health reaches zero.

### 2. Enemy AI
- **Enemy Behavior**: The enemy begins chasing the player when the player enters enemy's field of view.
- **Attack Mechanism**: Once in close range, the enemy attacks the player with melee strikes.
- **Health System**: The enemy also has a health bar that decreases when taking damage from the player.

### 3. Assets and Implementation
- **Free Assets**: Character models and animations were sourced from Mixamo or Unity Asset Store.
- **Custom Game Logic**: All game logic, including player controls, AI behavior, and combat mechanics, was implemented manually without using asset packs for logic.
