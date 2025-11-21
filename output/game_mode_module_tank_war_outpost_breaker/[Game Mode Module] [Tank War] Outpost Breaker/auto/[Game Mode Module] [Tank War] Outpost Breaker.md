# [Game Mode Module] [Tank War] Outpost Breaker

GAMEMODE OUTPOST BREAKER

# Phiên bản: v1.5

Người tạo file: $\textcircled{9}$ Kent (QuocTA)

Ngày cập nhật: 17 - 09 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>M6ta</td><td rowspan=1 colspan=1>Ngudi viet</td><td rowspan=1 colspan=1>Ngudireview</td><td rowspan=1 colspan=1>Duyet?</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>10-07-2025</td><td rowspan=1 colspan=1>Taofile</td><td rowspan=1 colspan=1>QuocTA</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>10-07-2025</td><td rowspan=1 colspan=1>Chinh thdi gian trän däu + dieu kien thängthua</td><td rowspan=1 colspan=1> phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.2</td><td rowspan=1 colspan=1>31-07-2025</td><td rowspan=1 colspan=1>Chinh lai base thanh outpost</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.3</td><td rowspan=1 colspan=1>06-08-2025</td><td rowspan=1 colspan=1>Dieu chinh gold</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.4</td><td rowspan=1 colspan=1>20-08-2025</td><td rowspan=1 colspan=1>Cap nhaät file dung voi design hien tai:- Bó passive gold tang giam dua vaooutpost bipha- Bó shop,chinh upgrade stats- Dieu chinh sang het Eng</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.5</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1>Kent</td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Mode Overview

1.1 Mode Name:

Capture Base (Chiếm Căn Cứ)

# 1.2 Team Format:

• 5v5

# 1.3 Match Duration:

• 4 minutes per match

# 1.4 Map Setup:

• 3 outpostbases positioned on the map • 2 teams set on the opposite sides of the map

# 2. Victory Conditions

2.1 Win Conditions:

• Attacking Team (ATK): Wins if they capture all 3 outposts within 4 minutes. Defending Team (DEF): Wins if they retain at least 1 outpost until time runs out.   
• Either Team: Wins if they can kill 20 enemies first.

# 2.2 Lose Conditions:

• Attacking Team (ATK): Loses if they fail to capture all outposts within the time limit.   
• Defending Team (DEF): Loses if they lose control of all outposts before the match ends.   
• Either Team: Loses if the other team meets 20 kills first.

# 3. Gameplay Flow

• Players are assigned to ATK or DEF teams pre-match.   
• Outposts start under DEF control. ATK must coordinate and advance to capture outposts sequentially or simultaneously.   
• Once an outpost is captured by ATK, it cannot be retaken by DEF.   
• Gold generation systems are introduced to accelerate match pacing and reinforce comeback mechanics.

# 4. Gold & Upgrade System

4.1 Gold Earning Sources:

• Enemy Tank Kill: Grants Gold for team.   
• Outpost Base Capture (ATK): Fixed Gold reward shared among all attackers.   
• Outpost Base Hold (DEF): Generates passive Gold per second per player while holding outpost.

# 4.2 Balance Design Philosophy:

• ATK Gold spikes with aggressive plays and coordinated captures.

• Both sides can achieve power scaling via kill streaks and team strategy.

• DEF Base close to outpost position

• ATK Base far from outpost position

# 4.3 Upgrade Shop:

• Players spend Gold during matches to upgrade:

◦ HP   
◦ Damage   
◦ Fire Rate   
◦ Movement Speed

# 5. UI/UX & Map Design

• Outpost Icons: Clearly marked A / B / C points on minimap, set position.

• Capture Progress Icon: Each outpost shows real-time capture status.

• Gold Counter: Visible for each player, shows earning rate and spend.

• Skill Slot: A skill slot for each class for quick activation with cooldown visuals.

• Upgrade Shop: Has 4 icons displaying side-by-side with gold value and stat value.

# 6. Gold Balance

# 6.1 Gold Sources In-Match (Calculate by Gold - g)

<table><tr><td rowspan=1 colspan=1>Event</td><td rowspan=1 colspan=1>ATK Team</td><td rowspan=1 colspan=1>DEF Team</td><td rowspan=1 colspan=1>Note</td></tr><tr><td rowspan=1 colspan=1> Tank takedown</td><td rowspan=1 colspan=2>100g (per person)</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Pha Outpost (ATK)</td><td rowspan=1 colspan=1>+300g/person/outpost</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Passive gold</td><td rowspan=1 colspan=2>+14g/s</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1> Kill streak (3 kills beyond)</td><td rowspan=1 colspan=2>+50g bonus</td><td rowspan=1 colspan=1> Streak rewardDoesn&#x27;t stack forfurther kill streaks</td></tr><tr><td rowspan=1 colspan=1> Streak ends</td><td rowspan=1 colspan=2>+100g bonus to the player that ends the streak</td><td rowspan=1 colspan=1></td></tr></table>

# 6.2 Gold flow estimate (Summarized)

<table><tr><td colspan="1" rowspan="1">Period</td><td colspan="1" rowspan="1">Gold total (Est. avg/person)</td><td colspan="1" rowspan="1">Est. objectives</td></tr><tr><td colspan="1" rowspan="1">0-1 mins</td><td colspan="1" rowspan="1">1200g-1600g</td><td colspan="1" rowspan="1"> Kill + Hold</td></tr><tr><td colspan="1" rowspan="1">1-3 mins</td><td colspan="1" rowspan="1">2000g-2400g</td><td colspan="1" rowspan="1">Snowball (ATK)A successful defense (DEF)</td></tr><tr><td colspan="1" rowspan="1">3-4 mins</td><td colspan="1" rowspan="1">3000g-4500g</td><td colspan="1" rowspan="1">Finish all upgrades</td></tr><tr><td></td><td></td><td>Finish pushing (ATK)</td></tr><tr><td></td><td></td><td>Complete 20 kills (DEF)</td></tr></table>

# 7. Conclusion

The "Capture Base" mode for Project TOG introduces a strategic tug-of-war between offense and defense. With asymmetric buffs and a resource-based upgrade system, it creates high-paced tactical gameplay, rewarding both coordination and moment-to-moment skill.

# 8. Scalability & Future Additions

• Map pools with different base layouts and terrain types.   
• Outpost-specific hazards (e.g., rotating turrets, flame walls).   
• Weather modifiers (fog, sandstorm) affect visibility.   
• Progression-based item unlock system.   
Weekly ranked mode with rotation between Capture Base and other PvP modes.   
• DEF teams can rebuild outposts by spending gold and time.