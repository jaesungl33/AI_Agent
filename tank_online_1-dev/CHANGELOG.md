# Changelog

> Generated from branch: `dev`
> Generated at: 2025-11-03 10:29:18

---

## 2025-11-03 (Monday)

### Bug Fixes

- bug  sync UI for shot `6581a2ef`

---

## 2025-10-31 (Friday)

### Bug Fixes

- change mode screen `b9ea3661`
- bug move at player dead `be79c5b6`
- fix countdown in gameplay `7135ca4d`
- hide draw/victory/defeat in final screen `21eea2c6`

### Others

- fix vfx radar prefab `be14f25a`
- edit scene level_optimized `1e1a043c`
- edit scale hierachy particle `eee5eaba`

---

## 2025-10-30 (Thursday)

### Features

- change mode `21c03c68`
- home screen - popup change mode `6b1365c7`
- new mode teamdeathmatch `c010a5be`

### Bug Fixes

- delay vfx spawn `96743c92`
- update ui of game play screen `9bd2a193`
- add delay 1 bit for load tank list `6266a5d1`
- heavy 3 `21c03c68`
- add new background music for home `126d7820`
- hide all butons of delete data (login screen, home screen) `19cea73f`
- adjust matchdocument `3fbb839e`
- add delay for transit to map loading state `3fbb839e`
- add draw for team battle mode `3fbb839e`
- URP (guide by Hai) `0aafd115`
- show winner for deathmatch & capture base `0aafd115`
- small bug of  find match `c010a5be`
- turret fade `ee76f26b`
- player spawn vfx `ee76f26b`

### Others

- add vfx radar tower `8efb6561`
- fix mesh B,C lane `57b56bc0`
- setup guide ABC (wip) `04c88bff`
- set layer wall, breakable for deathmatch map `2c6967a3`

---

## 2025-10-29 (Wednesday)

### Features

- respawn vfx `09f0adb5`
- turret indicator `d2d314a1`
- new skill for heavy 3 `9dfa1a5b`
- whirlwindchains icon `8f66ca5c`
- ability icon `a8297c3e`
- ability whirliwind chains collection `0c07f663`
- ability heavy 3 `e8208ae1`

### Bug Fixes

- update new UI for victory/defeat `49baf430`
- add auto collect rewards after game over `49baf430`
- add clear data after show final screen `49baf430`
- display tanks in choose tank screen & final screen `9dfa1a5b`
- game assets collection ability heavy3 `ff326527`
- bug show tanks in choose tank screen and final screen `f2cc2d2b`
- ability whirlwind `aef17e60`
- whirlwindchains param `93b5ec65`
- spawn effect `a8e80739`
- hpbar decor pos - heavy 3 preview tank `e8208ae1`
- update inventory manager (helper) `95eca019`
- change font of TankUI and Revival Text `95eca019`
- fix small bug in garage, allow buy tank, adjust position of buttons (modify, set lobby) `b27732d0`
- enhance inform popup (can attach message) `b27732d0`
- remove 2 tanks for default account `b27732d0`

### Others

- fix-art: update lighting scene `2183d41d`
- Update new tunnel, update script `7d22fd9b`
- fix animation, animator skill Heavy3 `74f72d6b`

---

## 2025-10-28 (Tuesday)

### Features

- garage `d2a2576b`

### Bug Fixes

- adjust photon config tickrate to 60 `e7b6a9ea`
- sync selected tank in choose tank screen `4174c61d`
- hide kamikaze seft dmg `d2a2576b`

### Improvements

- adjust logic for show tank list in choose tank screen `e7b6a9ea`
- change function name for EventManager from Emit to TriggerEvent `e7b6a9ea`

### Others

- NoActive xray for Breakable, default layer `ae96e79e`

---

## 2025-10-27 (Monday)

### Features

- Allow delete account `8c08ca75`

### Bug Fixes

- Adjust icon of tank in list to "preview icon" `8c08ca75`

### Improvements

- Enhance UIManager `8c08ca75`
- Enhance GameManager `8c08ca75`

### Others

- Disable Render Pipeline transparent `7850c6ea`
- Add vfx skill Heavy3, prefab demo_vfx `57d5cf65`
- Add animation Heavy3 `98d5aba5`

---

## 2025-10-24 (Friday)

### Features

- Tank collision explodes breakable objects `aa290272`
- add "coming soon" popup `a4f4b513`
- show "victory!" or "defeat!" message after gamover `a4f4b513`

### Bug Fixes

- tank base hide model heavy 3 `74c9f31e`
- tank health bar posistion `dd167065`
- tank dust doublicate `42a81f2d`
- adjust data for heavy 3 `369a2706`
- add heavy for new user (owner) `369a2706`
- can choose tank at Map Loading `14765cc4`
- time picking incorrectly `14765cc4`
- change logic for show tanks at match loading screen `14765cc4`
- input controller cancel area `8d6cc26c`
- game not end `cef57cb3`
- fix game not end `7c37f4af`
- rework input controller `2f8fee5c`
- update missing `17f970c4`
- adjust hp bar placement `a4f4b513`
- fix wrong status display during match search after return from garage `a4f4b513`
- update class icon for scout3 `a4f4b513`

### Others

- - feat art: Update heavy tank 3 Grinder, add Outpost ABC sign, fix collider deatchmatch `3ed350cf`
- add texture and material Heavy03 `44c2d651`
- Setup prefab tank_visual_heavy3, add animation skill `098d2bc4`
- fix art: fix map deathmatch collider `3c2d7617`

---

## 2025-10-23 (Thursday)

### Features

- ability load data `6958b576`
- inform popup craft `780d0d26`
- garage screen final `8b83e19c`
- garage deco screen 60% `8b83e19c`
- inventory manager final `8b83e19c`

### Bug Fixes

- changed game play scene to Capturebase `7fc34c0f`
- can not open Garage Decor `fa41b44b`
- bug of upgrade stat in game play screen `291886d8`
- disabled meshrenderer of tunel in map for capture base mode `291886d8`
- not update HP & MaxHP to network `291886d8`
- garate bug `780d0d26`
- added tag for tankbase_2 `c672c649`
- updated data for ability in game assets collection `8b83e19c`

### Others

- setup tank_visual Heavy3 `1a45ac21`

---

## 2025-10-22 (Wednesday)

### Features

- garage screen + garage deco screen + inventory WIP 2 `ec456352`
- change tank ui hp bar `2e416682`
- add icon ability storm `2e416682`

### Bug Fixes

- solved conflict and fix bug `6db71e31`
- conflict `6a7f05e2`
- tankbase prefab `45c12519`
- turret hp bar hide on dead `467f90f2`
- player list ui `9296400b`
- min slow 0 `2e416682`
- bug take damage `8984f7c3`

### Others

- setup rebder pipeline, material and script for Xray_tank `edec49ea`
- - feat art: Map deathmatch, fix scale tank `fd795e2b`

---

## 2025-10-21 (Tuesday)

### Features

- layout 2 screens (garage + garage deco) `b5d84434`
- added 2 scripts for dedicated server (dedicated server + client connector) `b5d84434`
- tank vfx visuals `67fb81fb`
- effect debuff slow - decrease fire rate `01b6c6f0`

### Bug Fixes

- input threshold `b446b07e`
- moved something from dedicated server `e24549b9`

---

## 2025-10-20 (Monday)

### Features

- ability storm `a59707e6`
- updated reward for player document (local + firebase) `8d4e5e43`

### Bug Fixes

- converter for firebase data (player document) `bddee9bf`
- login with guest ingored Dedicated_Server folder `bd47382b`
- can't end game after destroyed outpost `8d4e5e43`
- removed duplicate GamePlayDataEvent `8d4e5e43`
- read/write playerdatadocument/userdocument `8d4e5e43`

---

## 2025-10-17 (Friday)

### Features

- dash vfx hit `8594cb93`

### Bug Fixes

- kamikaze & change player ref id `82c6285c`

---

## 2025-10-16 (Thursday)

### Bug Fixes

- hook local visual `7d89f481`

---

## 2025-10-15 (Wednesday)

### Bug Fixes

- bug missing players after 2-3 times play `06e31498`
- mvp for all `720ddb39`
- player index incorrectly `c95fd913`
- get player by index `6d25a189`
- auto aim bush `6d25a189`
- player match data `f2c4773b`
- get player by index `6dadb5cd`
- avatar `6dadb5cd`
- auto aim `6dadb5cd`
- hook `6dadb5cd`
- position incorrectly in game `0a763edf`
- load match data incorrectly `5d0a5d0f`
- find collider for object in map `59256e8d`
- matchmaking final `cf871b84`
- matchmaking mechanics `c0892915`

### Others

- upgraded version to 1.2.4 `62f22fa9`

---

## 2025-10-14 (Tuesday)

### Bug Fixes

- refactoring the core game play mechanic of photon fusion 2 `3bfa4392`
- ignore noch loading scene `87550247`
- auto taget frame rate `87550247`
- outpot hitbox `0ee93791`
- fx normal atk `0ee93791`
- btn cancel ability `0ee93791`
- smoke scout 3 `0ee93791`

### Others

- reversed some thing `0e787116`
- fix trail_ground tiling `eaf8b692`
- add vfx_dash_impact `ff094e48`
- - fix art: Box, barrel collider, fix missing material in the center of the desert map, fix outline `6d91bbe0`

---

## 2025-10-13 (Monday)

### Bug Fixes

- btn upgrade `c940a52d`
- revert hook `dc93bfb1`
- turret hit breakable object `07fa1344`
- turret anim dead `86156d60`
- main turret fix missing materrial (hide line render unused) `012358d4`
- main turret hide red gun `264b65e9`
- add new player to proxy list `00cd75d2`
- kamikaze dmg outpot & breakable object `4b5d5dfc`
- show match list `ad50685a`
- add new player to list `ad50685a`
- ability dash `481556b5`
- ability stack icon `481556b5`
- player icon `481556b5`
- hook remove unused fx `481556b5`
- sync player data in match `a4983ae4`
- set level, elo, gold, diamond for new player `a4983ae4`
- added new state COUNTDOWN for ServerState `a4983ae4`
- update gold not yet `b5c90f26`
- increased fps for medium device `b5c90f26`
- play victory sound at FinalState `b5c90f26`
- disabled display state of choose tank screen `b5c90f26`
- changed order process in show gameplay screen (reset go first spawn turret icons) `b5c90f26`
- show tank list incorrectly `36509cbd`
- game over not work `36509cbd`
- duplicated tank at Home when find match `2d630bb8`
- your allies don't see your tank `2d630bb8`
- end match, find match then appear error `2d630bb8`
- master can change tank at map loading `2d630bb8`
- hang at loading screen `2d630bb8`
- can attack allies or set target or shot `2d630bb8`

### Others

- bug fixes for proxy player listt `4b622469`

---

## 2025-10-10 (Friday)

### Bug Fixes

- error of get_pixelsPerPoint `2ab0c091`
- firedash ability `daa55f3c`
- remake matchmaking mechanist & bug fixes & layout refine `26f5cd01`
- bug turret dead fire `6192e8a5`
- auto aim `99e1b501`
- camera size to 9 `99e1b501`
- ability handle effect hook `e20b965e`
- change color of countdown text `29b06559`
- solved bug of restart game & flow choose tank `ff028942`
- data incorrectly WIP `e4b810ce`

### Others

- auto shoot normal attack `d801428c`
- - feat art: Add tank outline. - fix art: Fix heavy tank visual `ddcdbbb2`
- - feat Art: fix heavy 2 display, add outline `eecd44cf`

---

## 2025-10-09 (Thursday)

### Features

- icon ability `b68010e6`
- ability dash effect applier `f6e2bd8b`

### Bug Fixes

- hook dmg, trap dmg `b68010e6`
- indicator normal atk `b68010e6`
- updated show total players in match `4dc5fbd3`
- show team/tager/kill in choose tank `f4513704`
- size of tank in home over UI `f4513704`
- format of font is wrong for "find match" button `f4513704`
- changed show ping to matchmaking manager `ab09ecc1`
- added ping for matchmaking manager `b552e09b`
- set player name with 12 characters `4f8cec97`
- fix layout (home, choose tank, final, match loading) `4f8cec97`
- added show ping `4f8cec97`
- replace panel tag icons for all tanks `4f8cec97`
- replace tag icon for all tanks `4f8cec97`
- changed limit (min) of max time to wait other players in choos tank screen `4f8cec97`
- reduce and update code for count down in lobby (waiting other player case) `4f8cec97`
- rollback trail_burn `3a62c3b1`
- host can't pick his tank `40f4cbf1`
- host can't choose tank `2f2fac36`
- can't choose assaut 2 `2f2fac36`
- removed icon for mini map on tank `2f2fac36`

### Others

- edit shader, prefab demo `86969d96`
- Revert "- art backup" `ca91fd37`
- - art backup `1abaa252`
- add vfx skill for StormBreaker(Assault3) `6c9a4d62`
- add animation skill for Assault3 `0b9fc76a`

---

## 2025-10-08 (Wednesday)

### Features

- ability fire dash fx `86ccbe7f`

### Bug Fixes

- remake matchmaking mechanist done `ef4ec212`
- removed tank preview at home screen `df622979`
- added process bar with real data at loading screen `df622979`
- added state machine for Matchmaking `b8c611a5`
- removed tank preview at home `b8c611a5`
- new matchmaking mechanist `b8c611a5`
- btn ability UI `10cdfc1b`

### Others

- setup prefab vfx 04 `92a2f6f2`
- setup prefab vfx 03 `deff8809`
- edit prefab muzzle, trail_bullet_default `3e34523d`
- edit pivot skill FireDash `19b48533`

---

## 2025-10-07 (Tuesday)

### Features

- ability-fire-dash `fa507396`
- indicator-circle-ground `fa507396`

### Others

- setup prefab vfx 02 `4ae181f8`
- setup prefab vfx `56207b7c`
- - feat art: add tank Assault 3 `42eedfab`
- update prefab lv00, lv03 `e88e8800`
- vfx_bullet_damage_upgrade (wip02) `bd18561d`

---

## 2025-10-06 (Monday)

### Features

- final match loading screen `3725514e`

### Bug Fixes

- turret dead `63c733f5`
- auto aim block by wall `63c733f5`
- show debug log null weapons `624ef2bf`
- auto fire normal attack `197af7ce`
- main camera size 13 -> 11 `197af7ce`

### Others

- vfx_bullet_damage_upgrade (wip) `f1461140`
- - fix art: Tank rotation pivot `ffca270f`

---

## 2025-10-03 (Friday)

### Features

- new screen Match Loading WIP `0be07088`
- anim & vfx turret destroy `1425ee91`

### Bug Fixes

- auto normal attack `06d175c7`
- move only rotate hull `c3de734e`
- move only rotate hull `63b4bc26`
- move only rotate hull `7c34cf9a`
- update range auto target `8fca14f4`
- load tank at Home with shadow + can rotate `7047ec1c`
- updated tank preview at home `5b3b3628`

### Improvements

- controller `1425ee91`

### Others

- edit animation, animator `48e5368a`
- edit vfx_tank_explosion `c0116aa2`
- edit vfx_tank_explosion, vfx_tank_damage `6d4d3cce`
- add vfx_explosion_object for assets, edit damage_05_destroyed `9b46f17d`
- - fix art: New bake light optimize (reduce lightmap quantity), add preview tank prefab, add dyn light scene (for testing), adjust tank size `a927e12b`
- art backup `c30a270b`
- - fix art: Tank scale `514a05d2`
- edit visual vfx_kamikaze `ec1e9ae6`

---

## 2025-10-02 (Thursday)

### Features

- Match loading WIP `951eaef8`

### Bug Fixes

- core gameplay game over `bb811418`
- auto aim `0da8e00e`
- beartrap fx `0da8e00e`
- postion of scout 3 `b5537821`
- removed 2 icons in home (for Tomato) `d97f2920`
- show teammate wrong in choose tank `d97f2920`
- show total players at home `d97f2920`
- cached old tank from last game `47ef683d`
- broken layout in Home Screen `47ef683d`
- show score of team incorrectly `47ef683d`
- update global volume & disable meshes of spawnpoint in map `b8cd7b86`
- auto aim `cfbb896c`
- enable find match button on home `951eaef8`
- hide indicator circle `34995211`
- kamikaze animation tank `206f3fbf`
- kamikaze animation tank `a46be530`
- beartrap hidden `d1705db5`
- update find match new for main menu `368186b2`
- update find/cancel find match `368186b2`
- update new graphic for reward popup (craft) `368186b2`
- change color for text  (gold, kill, death) in final screen `368186b2`
- bug can not start game `3ac674e3`

### Others

- add Vfx_Turret_Explosion, add animation dead turret `3056973d`
- - fix art: Lighting rebaked + Dynamic light scene `85c33db4`
- add vfx_explosion for asset `d12c64d0`

---

## 2025-10-01 (Wednesday)

### Features

- optimization testing `56325bcc`

### Bug Fixes

- hook vfx `a592dffa`
- joytick hide some case `6dfcf151`
- Add Toogle UI in map `a8e1f7c0`
- change tick rate of photon from 60 -> 30 `95e5e819`
- recheck for optimization (dup new scene for test) & adjust settings for quality and light `95e5e819`
- apply dynamic quality settings follow devices `95e5e819`
- hook vfx `4381b1b8`
- custom script shot `4381b1b8`
- auto target `4381b1b8`
- tank icon `b7a96099`
- controller cancel at fast tap `e1075c64`
- joytick skill Posistion `06729bfe`

### Improvements

- remake final screen `95e5e819`

### Others

- edit vfx_explosion_air, add vfx_explosion_object `8a7d88a8`
- add vfx respawn blue, red `d9c8975d`
- cleanup texture, material `fe682cb1`

---

## 2025-09-30 (Tuesday)

### Features

- added fps debug `c1d5b1d5`
- added new font  for shadow+outline `aabdab39`

### Bug Fixes

- hide tank explosion, kamikaze `ea91ef6a`
- left-joytick `45d5a478`
- Add gameobject toggle for debug `df878de6`
- hide indicator at normal attack `71e850ad`
- kill dead info `f8f04379`
- gameplay screen `f8f04379`
- changed mode of texture from repeat  to clamp `c1d5b1d5`
- removed some cameras in game `c1d5b1d5`
- ability show `04600671`
- player info `23e97c16`
- updated Home Screen layout fiix: updated Choose Tank Screen layout `aabdab39`
- completed choose tank screen `f1217a26`

### Others

- improve indicator `03fa9992`
- - feat art: update scout 3 `e57a0c5c`
- edit vfx visual `f62aee99`
- add animation kamikaze for Scout_02 `d6281021`
- feat-art: add vfx_dash, trail_burn and trail_ground `861b06c0`

---

## 2025-09-29 (Monday)

### Features

- add image gameplay ui `3cfeba11`
- updated tankselectitem prefab `005c48f6`
- added new font for burbankbig with underline `522c63e5`
- demo minimap `19272d78`

### Bug Fixes

- update responsive for login screen/ splash screen/ loading screen `25157c86`
- bugs show selected tank `25157c86`
- gameplay load player info `7ecf7784`
- ui upgrade `3cfeba11`
- layout ui player info `3cfeba11`
- icon coltroller `3cfeba11`
- update choose tank screen `37ba119b`
- start game 3 times `37ba119b`

### Improvements

- new ui for login screen `19272d78`
- new ui for home screen `19272d78`
- wip new ui for choose tank `19272d78`

### Others

- - feat art: Adjust map desert color. Adjust collider. Add transparent shader `0068cccb`

---

## 2025-09-26 (Friday)

### Features

- auto target `3ce27d17`
- load skill from tank collection `7d248f62`
- icon skill `7d248f62`

### Bug Fixes

- ability damage turret & vfx `7d248f62`
- hook fx `30a4c093`
- indicator image `416799af`
- ability hook visual `416799af`

### Others

- update vfx_dash (wip) `e72edd59`
- fix TA_Heavy_02_Skill_01_Skin_Prefab `bd48fc38`
- fix-art: update material for trail_bullet_default `ad95669c`
- skill_dash (wip) `b271b9dd`

---

## 2025-09-25 (Thursday)

### Features

- hook vfx `255482bc`
- vfx bear trap & animation `6847d594`

### Bug Fixes

- update scene level optimized from haibls `113f57a5`
- ability clear countdown `6847d594`
- ability hook `f4dfdcdc`

### Others

- - feat Art: Optimize Map Desert - Done `4ac66b9a`
- feat-art: add vfx_trap, prefab Demo_Tank_Trap `f75b0929`
- feat-art: add skin, animation, material TA_Assault_02_skill `d37bf7d2`
- - feat art: Combined mesh, reduce tris, draw call `b59ad0e2`

---

## 2025-09-24 (Wednesday)

### Features

- indicator-range `8ef68ed1`

### Others

- - feat Art: Update optimize shader `eab66f36`
- Art: backup wip `45eeba92`
- Update material Rope, relocate folder Heavy_02 `e73d8827`

---

## 2025-09-23 (Tuesday)

### Features

- player control info `5d9a7e48`
- ability hook `df3bd1e6`

### Improvements

- input controller - mobile input - helper of ability `5d9a7e48`
- ability base `df3bd1e6`
- effect base `df3bd1e6`
- kamikaze, bear trap `df3bd1e6`

### Others

- - feat art: convert mesh to impostor Wip `8b620f30`
- Move folder model Hook `45de5b69`
- fix-art: update anim hook `72a44a40`
- fix-art: update vfx_hook `de4b01c0`
- feat-art: vfx hook (wip) `183f8ed3`
- - feat art: Optimize Map desert WIP2 `83e0d77d`

---

## 2025-09-22 (Monday)

### Bug Fixes

- light setting android - aim controller `25239554`

### Improvements

- bear trap remove oldest trap `0f689266`
- ability bear trap `f299988c`

### Others

- - feat art: Map desert optimize WIP1 `744217fa`
- kamikaze vfx, shot, dust vfx `be420d56`

---

## 2025-09-19 (Friday)

### Features

- kamikaze vfx `5a69783e`
- effect applier, ability bear trap `0ccae9ef`

### Bug Fixes

- sparse collection deactive shot `5a69783e`
- tank base animation, player control `0ccae9ef`

### Improvements

- dust tank `5a69783e`
- ability base `0ccae9ef`

### Others

- -feat art: Update optimize - grass (wip) `6684a119`
- feat-art: add vfx_trap_bear `9feb716c`
- - feat art: update tank avatar `004d3b7a`
- - feat art: Update skin default for Tank Scout 2, Assault 2, Heavy 2 `a09d165d`

---

## 2025-09-18 (Thursday)

### Others

- fix prefab tank base `be7eabb1`
- add prefab tank visuals (scout2) `aac968e9`
- - feat art: update skin Heavy + Assault default `a0f3e0de`
- feat-art: vfx Kamikaze `a84a133c`
- indicator improve `5f24426a`
- player control & indicator comleted `97dbadef`
- feat-art: vfx kamikaze wip `ae993cef`

---

## 2025-09-17 (Wednesday)

### Others

- - feat art: Update tank Heavy 2. Update Tank Shader wth Dust + Wrap Layer `5bf2474b`

---

## 2025-09-16 (Tuesday)

### Others

- - feat art: Update Desert map adjust spawn point + Layout `6a8fc768`
- - feat art: Map desert adjust spawn position. Add new env assets `822748ba`

---

## 2025-09-15 (Monday)

### Others

- feat-art: add vfx_dust_normal, vfx_tank_damage, vfx_tank_explosion `1eb06026`

---

## 2025-09-11 (Thursday)

### Features

- new feature hide on bushes `04975d88`
- get to main menu if has player left `04975d88`

### Bug Fixes

- update vfx (bullet) `0fcb9b90`
- update button for find-match in mainmenu screen `c6a2f765`
- added delay 1s for find/exit match `1b8e057d`
- hide in bushes `1b8e057d`
- can't shot if click upgrade area `04975d88`

### Others

- - feat art: update map Desert add Tunnel. Change terrain's color `60272861`
- feat-art: add vfx tank (trail bullet, explosion_air, explosion_ground) `495ffcfb`
- - feat art: edit map Desert Layout. Add tunnel `b5bcbdc2`
- ability visual `46c6663b`
- feat-art: add textures, mesh `94c99e0d`
- aoe dmg hit turret `fded6a1d`
- base ability `bfb5cbaa`

---

## 2025-09-10 (Wednesday)

### Bug Fixes

- hide in bush `0c7f9abd`
- updated config for photon fusion 2 `40bc6f90`
- updated readme file for project  structure + module structure `40bc6f90`

### Others

- updated changelog `5a8fddb2`
- - feat art: update map setting + set up lighting for 2 tank - assault 2. + scout 2 `db8588bd`

---

## 2025-09-09 (Tuesday)

### Features

- updated changelog.md + script to create changelog `fc777e69`

### Others

- - feat art: Update tank assault 2 + tank scout 2 `2703ab26`
- Update file README.md `9babd90d`
- Update .gitlab-ci.yml file `8faeb5cf`
- Update .gitlab-ci.yml file `7c954756`
- Update .gitlab-ci.yml file `7afbc1cf`
- -feat art : fix level map (WIP) `16935adc`

---

## 2025-09-08 (Monday)

### Features

- new graphics for loading/splash/login `ac9790f8`
- new game icons `ac9790f8`
- new fonts `ac9790f8`

### Bug Fixes

- find & exit quick `664d160e`
- apply new font for mainmenu/loading/login/finalscreen/lobby `72f2d88c`
- removed outline and underlay for normal font `72f2d88c`
- tank move with isometric `a43f97a2`
- upgraded to unity 6.2 `a66a2651`

---

## 2025-09-07 (Sunday)

### Bug Fixes

- apply look at camera for new view `dc4bbf9d`

---

## 2025-09-05 (Friday)

### Bug Fixes

- Can not load game scene `60099d85`
- control tank with new camera view `60099d85`
- hp/max-hp not exactly after change tank `60099d85`
- can attack allies `60099d85`
- allowed close the room after final game `a526761e`
- hided hp bar after dead `a526761e`
- all configs can be loaded from remote config (exclude matchmaking) `a526761e`
- added waiting time to load managers at startup of GameManager `a526761e`

---

## 2025-09-04 (Thursday)

### Features

- added gizmos debug for target and range of tank + turret `9f132868`

### Bug Fixes

- update range of shot for turret + tank (range calc from center of object) `9f132868`
- adjusted size of audio sources for all objects in scene `9f132868`
- updated weapon data for tank `ba644884`
- removed gravity for all bullets `ba644884`
- destroy bullet if out of range `ba644884`
- click upgrade button in air `ba644884`

### Improvements

- adjusted camera to isometric 100% `588a11e7`

---

## 2025-09-03 (Wednesday)

### Features

- changed host if host left the game `2f656ff2`

### Bug Fixes

- added async for load scene `2f656ff2`
- holed player in room until end of the game, player can play another game by find new match `2f656ff2`
- reduced size of top-center bar in game play `cbafed4d`
- click upgrade in air `cbafed4d`
- WIP batch 4 `2644b476`

---

## 2025-08-30 (Saturday)

### Bug Fixes

- WIP batch 3 `9f734cf2`

---

## 2025-08-29 (Friday)

### Bug Fixes

- WIP batch 2 `a2d23cbc`
- WIP Batch 1 `7921263e`

---

## 2025-08-28 (Thursday)

### Features

- show respawn time after death `d0701c8c`

### Bug Fixes

- display 2 shadows of outpost `d0701c8c`
- dont update kill/death of team `d0701c8c`
- adjust position of damage text for outpost `d0701c8c`
- updated small bugs `9264f687`

---

## 2025-08-26 (Tuesday)

### Features

- auto target `968889eb`

### Bug Fixes

- adjust collider's size of outpost `968889eb`
- upgrade wrong data when play again `968889eb`
- no display visual of tank `968889eb`

---

## 2025-08-22 (Friday)

### Features

- final hp bar for tank `28f119c1`
- new HP Bar for tank `5ac54237`

### Bug Fixes

- adjust rotation of UI for object in map `14ee33c6`

### Improvements

- update hp bar of outpost `14ee33c6`
- add occlusion culling for Outpost Breaker Mode `14ee33c6`
- removed shadow of grass to test performance `14ee33c6`
- updated animation of damage text when be hit `28f119c1`

### Others

- fix-dev: Fix bug firebase init `6cae09b3`

---

## 2025-08-21 (Thursday)

### Features

- WIP update new HP UI `4f9ec91f`

### Bug Fixes

- #69 some objects apear in the center of map after destroyed by tank `6a89570e`
- An issue of hp bar flicker on tank `b12192b0`

### Others

- no message `a4e548d1`
- feat-art: up model envi and update scene `1c422ef2`
- feat-dev: add reward popup `480e8a24`

---

## 2025-08-20 (Wednesday)

### Bug Fixes

- hang game when restart #72 `41229ace`
- hang game when restart and host left (WIP) `571fa8e4`
- WIP #72 hang g ame when restart game or host left `b7c1a9e8`

### Improvements

- init data and sync from master to all clients (player) `41229ace`

### Others

- fix-dev: Fix bug Fusion/Firebase `5f30c8a7`

---

## 2025-08-19 (Tuesday)

### Bug Fixes

- hang game `a2171739`
- changed color of area for outpost `e300f612`
- Display wrong range of outpost but still work well #73 `906c85b9`

### Improvements

- Player & FusionPlayer `322019ad`
- PlayerData in match `322019ad`
- CoreGamePlay `322019ad`
- changed app id of photon fusion 2 #74 `f7577e8d`

---

## 2025-08-18 (Monday)

### Features

- added gm tool v1 `66ab5f47`

### Bug Fixes

- bug hp of tank not reduce (upgrade will appear) `86f1fbfb`
- updated animator for turret `3b97247b`
- animation of outpost not fit firerate of weapon `b922e7a1`
- increased height of boxcolider for tank `b922e7a1`
- bug of input controller (pc + android) `876bb17f`

### Others

- feat-dev: Add UI Main Menu Add logics for Firestore/RemoteConfig Collections `a91ae8ff`
- fix-dev: Turret icons update `d2db2f3a`
- feat-dev: Set up turret id on score board `3ff455f7`

---

## 2025-08-17 (Sunday)

### Bug Fixes

- bug of input controller for mobile `98995bc4`

---

## 2025-08-15 (Friday)

### Bug Fixes

- auto set target for outpost `0a35d20f`
- hang 3s at startup `54a9f37e`
- removed stars `54a9f37e`
- removed raycast at HP bar `54a9f37e`
- matchmaking incorrectly `06943f7c`

### Improvements

- final phase 1 `31fbb968`

### Others

- fix-dev: Fix tank icon `3d6a4744`
- fix-dev: Fix UI Image `d4581e01`
- feat-dev: adjust tank icon and add default avatar `813275ef`

---

## 2025-08-14 (Thursday)

### Features

- new data for upgrade tank stats `1bf11359`

### Bug Fixes

- damage of outpost incorrectly `ec2ebd9e`
- apply damage from Player or Turret (not for prefab) `ec2ebd9e`
- increased size of collider for tank `ec2ebd9e`
- click through UI with fire `d9fcd00b`
- converted weapon to old version `d9fcd00b`
- visuals of turret + tank `1bf11359`

### Improvements

- removed log for warning and logs, keep error `b51daec2`
- update tank selection, update tank stats, update weapon for scout1, assault1 `fd3bed15`

### Others

- feat-dev: Add final screen `84db3afa`
- fix-dev: lobby loading bug `f636ff60`
- feat-dev: Update upgrade logics `153f2f4f`
- WIP for weapon + shot of tank `f73b722e`

---

## 2025-08-13 (Wednesday)

### Bug Fixes

- show tank in match wrong `c4d261e8`
- bug show HUD of tank in match `afef2251`
- update wrong data for players `afef2251`

### Improvements

- final tank stats `afef2251`

### Others

- fix-dev: Game play turret icon bug `1a46b5cf`
- WIP for tank visuals `a3e07a84`
- fix-dev: Fix layout lobby screen on iPad `e1cfb1f7`
- fix-dev: fix lobby title `626411f1`
- fix-dev: Fix tank select `45e8eb86`
- fix-dev: Fix input direction and attacker/defender position in lobby `85195aa2`
- feat-dev: Add match target in lobby `70050375`

---

## 2025-08-12 (Tuesday)

### Bug Fixes

- Bug load tank data `224d10b1`

### Improvements

- WIP tank stats `cd7a04ac`
- WIP tank stats & all visuals of tank `586857d5`

### Others

- feat-dev: Add select tank logics `394b712c`

---

## 2025-08-11 (Monday)

### Improvements

- WIP tank stats and management match data `028789a7`
- WIP to fix missing id some tanks `cd9e223e`
- WIP for tanks management `e68984c4`
- Player Match Data + Upgrade Mechanic (WIP) `8135ed21`

### Others

- featt-dev: Add lobby select tank layout and countdown logic `54ee2edd`
- fix-art: fix skin and aniamtion Tower `1eeb4ece`
- fix-dev: Auto Firing bug `09e10f26`
- fix-dev: fix bug joystick direction `e7662b26`

---

## 2025-08-10 (Sunday)

### Improvements

- tank components `1c197c63`
- player data in match `1c197c63`
- networked properties for player in match `1c197c63`
- data for player in match `1c197c63`

---

## 2025-08-08 (Friday)

### Features

- Gameplay + UI `98efe3e7`
- tank upgrade stats in match `4feca911`
- outpost stats `4feca911`
- update stats for tank in match `7bd4dbbe`

### Bug Fixes

- gameplay screen has error with reset func `b36366ea`
- reverse control on mobile `b36366ea`
- can't upgrade skill `c44f1bb2`
- add boxcollider for tank `7bd4dbbe`
- moved Dat's sample folder `cf1e57c8`

### Others

- fix-dev: joystick position bug `93b4eab1`
- fix-dev: Update UI upgrades `8d6ff4f0`
- feat-dev: Fix interaction Upgrades `ef453b61`
- feat-dev: Add flow upgrade `f09fdaf9`
- feat-art: add new tank - assault, fix map layout `2119620e`
- feat-dev: Add Gameplay UI logic update `b0189ea2`
- feat-art: add heavy tank 01 `855a345d`

---

## 2025-08-07 (Thursday)

### Features

- added backup scene for gameplay `7f016ede`
- +gold for kill `7c7475ca`
- +gold for destroy turret `7c7475ca`
- condition win (20 kills,  3 turrets, time) `7c7475ca`
- final screen mockup `7c7475ca`
- added final state `7c7475ca`

### Bug Fixes

- Tank turret rotation pivot, Tank cannon mesh `fb736dc8`
- update tank shader `f20ed916`

### Others

- feat-dev: Add new UI sprites `b7daaa71`
- fix-art: edit animation, add effect sample `b92f8c45`
- feat-art: add skin, animation Tower `f84f7a5e`

---

## 2025-08-06 (Wednesday)

### Features

- new tank (scout 1) `7fd337e1`

### Bug Fixes

- dont send Player to show stats `fd50e247`
- bug 24 bytes `1b3b28b8`
- update room name for matchmaking mechanic `6979de78`
- sync state incorrect for turret `6979de78`
- destroyed tower but enemy not found `6979de78`
- WIP turret mechanic `e5526e62`

### Improvements

- removed some code unused `6979de78`

### Others

- feat-dev: Add game play screen layout `abcaa8d0`
- feat-dev: add config and fix bug `90a78666`

---

## 2025-08-05 (Tuesday)

### Features

- Tank Scout 01 `8fe71c68`

### Bug Fixes

- WIP bug of turret `55c21d39`

### Others

- feat-dev: Add team status UI in game play screen `3f5c176d`
- 1/ fix lỗi trụ tấn công đồng minh hoặc không tấn công ai (nguyên nhân do detect sai vì có box collider xung quanh nó) 2/ fix lỗi trượt của tank khi di chuyển theo phản hồi của Quốc 3/ cải thiện mức độ giật của camera cho Duy đỡ chóng mặt 4/ Thử nghiệm khóa  nòng để bắn 5/ Mở lại cho phép xoay nòng 6/ Build thử android 7/ Sửa lỗi đạn bắn xuyên hàng rào (do lỗi Layer) 8/ Tăng radius va chạm của viên đạn `8c744a13`

---

## 2025-08-04 (Monday)

### Bug Fixes

- change spawn position `eaa807a4`
- bug projectile through wall `f9bd96e8`
- changed turret in map `faf4c36a`
- changed mechanic for hide in bushes `faf4c36a`
- bug shot allies of turret `faf4c36a`
- bullet shader + Post processing `93783a0d`
- change stats of tank `f8f2b5d7`

### Improvements

- lock  turret `f9bd96e8`
- changed rotation speed of camera `faf4c36a`
- changed hull's position of tank `faf4c36a`

### Others

- merged with Son Hai! `92fb22d7`
- Update bullet Shade `f5c8dbcd`
- feat -Map desert update lighting `02a08984`

---

## 2025-08-03 (Sunday)

### Features

- tanks can hide in bushes `028b448e`

### Bug Fixes

- change hud rotation for player info in match `028b448e`
- camera angle is not correct `028b448e`
- lobby screen shows incorrect player list `028b448e`
- turret checking for attack allies `028b448e`

---

## 2025-07-31 (Thursday)

### Features

- show HUD of tank in match `58d9c789`
- updated matchmaking: show lobby with countdown `74dcb0e0`
- updated countdown in game `74dcb0e0`
- show lost HP `74dcb0e0`
- auto find target and auto shot for OUTPOST `74dcb0e0`
- increase gold in game for everyplayer `74dcb0e0`
- new map desert `ea26425a`

### Bug Fixes

- lobby `58d9c789`

### Others

- Merged with Son Hai (map desert) `238a8dcd`

---

## 2025-07-30 (Wednesday)

### Features

- turret final (find targets, damage them, show damage to tank, load data from configs, new weapon for turret) `aad9203f`

---

## 2025-07-29 (Tuesday)

### Features

- WIP turret mechanic `664cb3ba`
- map + layout + convert into URP `c9cdba8c`
- WIP turret mechanic `07ca4d27`

### Others

- update spawn positions `d507acf8`

---

## 2025-07-28 (Monday)

### Features

- new gameplay ui `45db641e`
- WIP turret single shot `45db641e`
- WIP for K/D in match `999bda4f`

### Others

- feat WIP scoreboard (kill/death/outpost destroyed) `fe3f58cb`

---

## 2025-07-27 (Sunday)

### Features

- team match & attack `c742decb`
- camera for team 1 & 2 `c742decb`

### Bug Fixes

- Color is incorrectly `c742decb`
- Response in 10 seconds `c742decb`

---

## 2025-07-25 (Friday)

### Features

- updated data for tank, player, match, team match, game `286f1946`

---

## 2025-07-24 (Thursday)

### Features

- game data, team data, win condition data `03aafed2`
- final lobby screen for max 10 players `786be4c0`
- show the player name in match for player `786be4c0`
- change the camera follow the team `786be4c0`
- create the player in match follow the team, max 10 players `786be4c0`

---

## 2025-07-22 (Tuesday)

### Features

- team match & battle `da9d51e2`

### Improvements

- upgrade lobby screen with playername & tankname & avatar `da9d51e2`
- upgrade Matchmaking Document, Tank Document `da9d51e2`

---

## 2025-07-21 (Monday)

### Bug Fixes

- matchmaking system `a0f9a687`
- matchmaking document `a0f9a687`
- shop proximity check removed `5f6e8b80`

---

## 2025-07-18 (Friday)

### Others

- WIP `f68ebffd`
- Updatet States `84d5449e`
- Merged matchmaking + gameplay `91f3e95f`

---

## 2025-07-17 (Thursday)

### Bug Fixes

- scene `9aa6e849`

### Others

- WIP `1c0453b8`
- WIP `e99b6b55`
- Tạo mock-up của map `06f46ac9`
- shop UI done done `109cf8e6`
- shop UI done `8410c5f9`

---

## 2025-07-16 (Wednesday)

### Bug Fixes

- upgraded state machine system `639e1659`
- missing components in tank `639e1659`

### Others

- Update statemachine `b1e9e6f4`

---

## 2025-07-14 (Monday)

### Refactoring

- GamePlayState, MatchmakingState, MainMenuState, LoadingState, DatabaseManager, GameDatabase `6e39ef0a`

### Others

- sua input luc mo shopUI `c7c4736d`
- sua input luc mo shopUI `3893e46f`

---

## 2025-07-11 (Friday)

### Features

- Lobby screen - `a0148a88`
- Loading screen - `a0148a88`
- Home screen - `a0148a88`
- Matchmaking system - `a0148a88`
- integrate graphics for loading - `14f87d96`
- final matchmaking system `14f87d96`

### Bug Fixes

- Bug with find match and cancel match `a0148a88`

### Others

- shop done `e7403103`
- update ignore `dd399889`

---

## 2025-07-10 (Thursday)

### Others

- WIP: - Matchmaking system - Find match/ Cancel match `c19a4429`

---

## 2025-07-09 (Wednesday)

### Others

- WIP - UI Manager & Fusion System - Matchmaking Screen `f00da5a9`

---

## 2025-07-08 (Tuesday)

### Others

- WIP - Matchmaking & UIs `0fe8fcc1`
- ﻿WIP `f42fdc62`
- wip - Main Screen - Login Screen - Matchmaking Screen - Observer for UIManager `e81a8b98`

---

## 2025-07-07 (Monday)

### Others

- - WIP + Login with guest + User profile + UI Template + UI Base Coding `f940e989`

---

## 2025-07-02 (Wednesday)

### Others

- Update game play: - fix camera - fix 10 positions in game play wrong - applied new map - level 3 `e8f5b775`
- Update Tank Module `6315dcb9`

---

## 2025-07-01 (Tuesday)

### Features

- Display player name in match `991bfaa8`

### Others

- WIP: - Matchmaking `bd01c637`
- cached for plugins/android `2c3d511d`
- ignored - firebase folder - generated local repo `ddc8856c`

---

## 2025-06-30 (Monday)

### Others

- Added rect tool `3be9ec3c`
- update data for 5 components of tank `4d21c1bf`

---

## 2025-06-29 (Sunday)

### Others

- - Added firebase (analytics, firestore, crashlytic) - Added in-app purchasing of unity - Defined some databases for game with (document & collection) `b332150b`

---

## 2025-06-27 (Friday)

### Others

- Update với version của /dev và add block out của map mới (+ vài prefabs và materials) `6cf5947f`

---

## 2025-06-26 (Thursday)

### Others

- WIP for database `c6dedf3d`
- Completed for refactoring ragnarok to our game `5a045ba7`

---

## 2025-06-25 (Wednesday)

### Others

- WIP: Database for game `52c37435`

---

## 2025-06-23 (Monday)

### Others

- WIP: Updating ragnarok project + tank project `5d495189`

---

## 2025-06-20 (Friday)

### Others

- added source code "ragnarok" `7723a584`

---

## 2025-06-19 (Thursday)

### Others

- removed textmesh pro, upgrade to unity 6 `b1024ce7`

---

## 2025-06-18 (Wednesday)

### Features

- observer pattern - `4a3e9693`
- singleton partern - `4a3e9693`
- hotween plugin - `4a3e9693`
- state machine system & statemachine for game/ui - `4a3e9693`
- game data for game/player - `4a3e9693`
- tank selection ui `4a3e9693`

---

## 2025-06-17 (Tuesday)

### Others

- update new structure! `25eb16f8`

---

## 2025-06-16 (Monday)

### Others

- init project! `63469671`
- Initial commit `c20c7176`

---

---

**Total:** 718 commits across 97 days

