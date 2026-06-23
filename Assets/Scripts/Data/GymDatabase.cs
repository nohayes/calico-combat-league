using System.Collections.Generic;

public static class GymDatabase
{
    public static readonly List<GymInfo> AllGyms = new List<GymInfo>
    {
        BuildBoxingGym(),
        BuildMuayThaiGym(),
        BuildWrestlingGym(),
        BuildBjjGym(),
        BuildChampionshipGym()
    };

    static OpponentInfo CreateOpponent(string id, string name, List<MoveData> moves,
        int health, int stamina, int strength, int defense, int speed, int striking, int grappling, int submission,
        int rewardXP, int rewardCoins, string nickname = "", string quote = "", string description = "",
        string bio = "", string lossLine = "", string winLine = "")
    {
        return new OpponentInfo
        {
            OpponentId = id,
            Name = name,
            Moves = moves,
            RewardXP = rewardXP,
            RewardCoins = rewardCoins,
            Nickname = nickname,
            Quote = quote,
            Description = description,
            Bio = bio,
            LossLine = lossLine,
            WinLine = winLine,
            Stats = new FighterStats
            {
                MaxHealth = health,
                CurrentHealth = health,
                MaxStamina = stamina,
                CurrentStamina = stamina,
                Strength = strength,
                Defense = defense,
                Speed = speed,
                Striking = striking,
                Grappling = grappling,
                Submission = submission
            }
        };
    }

    static GymInfo BuildBoxingGym()
    {
        var moves = MoveDatabase.BoxingTrainerMoves;

        return new GymInfo
        {
            GymId = "boxing_gym",
            GymName = "Boxing Gym",
            GymType = GymType.Boxing,
            Description = "Sharp hands and footwork. Where every champion learns to throw a punch.",
            Motto = "Hands up, hearts higher.",
            History = "Founded by a dock worker turned title contender, the Boxing Gym still trains fighters in the same converted warehouse where it all began.",
            RequiredGymId = null,
            UnlockMoveId = MoveDatabase.Hook.Id,
            Trainers = new List<OpponentInfo>
            {
                CreateOpponent("boxing_trainer_1", "Trainer Chuck", moves, health: 80, stamina: 40, strength: 8, defense: 7, speed: 8, striking: 10, grappling: 5, submission: 4, rewardXP: 15, rewardCoins: 10,
                    nickname: "The Brick Wall", quote: "Hope you brought a lunch. This might take a while.", description: "Immovable in the pocket, Chuck out-walls everyone who tries to out-punch him.",
                    bio: "Works construction during the day. Boxes every night.", lossLine: "Hah! Okay, that's fair. Good fight, kid.", winLine: "Construction by day, lessons by night. No charge."),
                CreateOpponent("boxing_trainer_2", "Trainer Dustin", moves, health: 90, stamina: 42, strength: 9, defense: 8, speed: 9, striking: 11, grappling: 6, submission: 5, rewardXP: 20, rewardCoins: 12,
                    nickname: "Quick Hands", quote: "I've already pictured this fight. I win. It's a good picture.", description: "Dustin throws combinations faster than most fighters can think.",
                    bio: "Claims he invented cardio. Nobody can prove otherwise.", lossLine: "Okay WOW. Did NOT picture that one.", winLine: "Called it. I called it in my head. You believe me, right?"),
                CreateOpponent("boxing_trainer_3", "Trainer Whit", moves, health: 100, stamina: 45, strength: 10, defense: 9, speed: 9, striking: 12, grappling: 6, submission: 5, rewardXP: 25, rewardCoins: 15,
                    nickname: "Iron Jaw", quote: "You hit like a broken treadmill. I've heard the rumors.", description: "Whit's chin has never been the same fighter's problem twice.",
                    bio: "Has never lost an argument. Working on fights next.", lossLine: "...There's no counter-argument for that. None.", winLine: "I told you I don't lose. Arguments OR fights.")
            },
            Leader = CreateOpponent("boxing_leader", "Gym Leader Mouse", moves, health: 130, stamina: 55, strength: 12, defense: 11, speed: 11, striking: 15, grappling: 7, submission: 6, rewardXP: 60, rewardCoins: 30,
                nickname: "The Closer", quote: "Round one's just the introduction. Stick around for the ending.", description: "Mouse doesn't chase knockouts. He waits for the mistake that gives him one.",
                bio: "Everyone in the gym wants a shot at the leader's chair. Most regret asking for one.", lossLine: "Hah! Finally - someone who reads past round one.", winLine: "Closer than I expected. That's not a compliment. It's a warning.")
        };
    }

    static GymInfo BuildMuayThaiGym()
    {
        var trainerMoves = MoveDatabase.MuayThaiTrainerMoves;
        var leaderMoves = MoveDatabase.MuayThaiLeaderMoves;

        return new GymInfo
        {
            GymId = "muaythai_gym",
            GymName = "Muay Thai Gym",
            GymType = GymType.MuayThai,
            Description = "The art of eight limbs. Knees and elbows turn distance into damage.",
            Motto = "Eight limbs, one will.",
            History = "Built on the teachings of a traveling Thai instructor, this gym treats every strike - fist, elbow, knee, shin - as part of the same conversation.",
            RequiredGymId = "boxing_gym",
            UnlockMoveId = MoveDatabase.SpinningBackKick.Id,
            Trainers = new List<OpponentInfo>
            {
                CreateOpponent("muaythai_trainer_1", "Trainer Niran", trainerMoves, health: 110, stamina: 48, strength: 10, defense: 10, speed: 11, striking: 13, grappling: 8, submission: 6, rewardXP: 30, rewardCoins: 18,
                    nickname: "Whip Leg", quote: "I won't say much. My leg does the talking.", description: "Niran's low kicks have ended more fights than his hands ever will.",
                    bio: "Trains barefoot on gravel. Says shoes are cheating.", lossLine: "My leg... remembers this. So will I.", winLine: "Told you. The leg talks."),
                CreateOpponent("muaythai_trainer_2", "Trainer Somchai", trainerMoves, health: 120, stamina: 50, strength: 11, defense: 10, speed: 12, striking: 14, grappling: 8, submission: 6, rewardXP: 35, rewardCoins: 20,
                    nickname: "The Elbow", quote: "Personal space is a Western invention. Get comfortable.", description: "Somchai turns the clinch into a minefield.",
                    bio: "Considers personal space a Western invention.", lossLine: "You found space I didn't know existed.", winLine: "Too close, too late. That's the whole gameplan."),
                CreateOpponent("muaythai_trainer_3", "Trainer Anan", trainerMoves, health: 130, stamina: 52, strength: 12, defense: 11, speed: 12, striking: 15, grappling: 9, submission: 7, rewardXP: 40, rewardCoins: 22,
                    nickname: "Iron Shin", quote: "Ten thousand kicks thrown. You're about to be kick ten thousand and one.", description: "Anan trades leg kicks like he's allergic to losing.",
                    bio: "Counts every kick he's ever thrown. Loses count after ten thousand.", lossLine: "Huh. Add that one to the count too, I guess.", winLine: "New number. Same result.")
            },
            Leader = CreateOpponent("muaythai_leader", "Gym Leader Sakda", leaderMoves, health: 160, stamina: 65, strength: 14, defense: 13, speed: 14, striking: 18, grappling: 10, submission: 8, rewardXP: 90, rewardCoins: 45,
                nickname: "The Storm", quote: "I don't fight in rounds. I fight in waves. Hope you can swim.", description: "Sakda overwhelms with volume until there's nothing left to defend.",
                bio: "Trained under a teacher who never raised his voice. Sakda kept the calm. Lost the restraint.", lossLine: "...The storm passes eventually. Today was the day.", winLine: "The storm doesn't apologize. Neither do I.")
        };
    }

    static GymInfo BuildWrestlingGym()
    {
        var trainerMoves = MoveDatabase.WrestlingTrainerMoves;
        var leaderMoves = MoveDatabase.WrestlingLeaderMoves;

        return new GymInfo
        {
            GymId = "wrestling_gym",
            GymName = "Wrestling Gym",
            GymType = GymType.Wrestling,
            Description = "Control the clinch, control the fight. Takedowns and top position rule here.",
            Motto = "Control the fight. Control the outcome.",
            History = "Once a small-town high school wrestling room, now a breeding ground for fighters who simply refuse to be moved.",
            RequiredGymId = "muaythai_gym",
            UnlockMoveId = MoveDatabase.Suplex.Id,
            Trainers = new List<OpponentInfo>
            {
                CreateOpponent("wrestling_trainer_1", "Trainer Kurt", trainerMoves, health: 140, stamina: 55, strength: 13, defense: 12, speed: 12, striking: 14, grappling: 14, submission: 8, rewardXP: 45, rewardCoins: 25,
                    nickname: "The Anchor", quote: "Every problem in my life has been solved with a double leg. This one too.", description: "Kurt's takedowns aren't flashy. They're inevitable.",
                    bio: "Believes every problem can be solved with a double leg.", lossLine: "Gravity owed me one. It's still on my tab.", winLine: "Gravity. Undefeated. Just like me."),
                CreateOpponent("wrestling_trainer_2", "Trainer Brock", trainerMoves, health: 150, stamina: 57, strength: 14, defense: 13, speed: 13, striking: 15, grappling: 15, submission: 9, rewardXP: 50, rewardCoins: 28,
                    nickname: "Big Country", quote: "I once carried a fridge up three flights of stairs. You're lighter than the fridge.", description: "Brock's strength advantage shows up the moment the fight hits the mat.",
                    bio: "Once carried a refrigerator up three flights of stairs. For fun.", lossLine: "Okay. You're heavier than the fridge. Respect.", winLine: "Heavier than the fridge, lighter than my standards."),
                CreateOpponent("wrestling_trainer_3", "Trainer Danny", trainerMoves, health: 160, stamina: 60, strength: 15, defense: 14, speed: 13, striking: 16, grappling: 16, submission: 9, rewardXP: 55, rewardCoins: 30,
                    nickname: "Scramble", quote: "Stand still. I dare you. You won't. Nobody does.", description: "Danny never stops moving, never stops chaining.",
                    bio: "Hasn't stood still since 2019.", lossLine: "Wait, you stood still on purpose? That's... actually smart. Rude, but smart.", winLine: "Told you not to stand still. Nobody listens to Danny.")
            },
            Leader = CreateOpponent("wrestling_leader", "Gym Leader Ivanov", leaderMoves, health: 190, stamina: 75, strength: 17, defense: 16, speed: 15, striking: 18, grappling: 20, submission: 10, rewardXP: 120, rewardCoins: 60,
                nickname: "The Vice", quote: "Escape is a theory. Tonight, I disprove it.", description: "Ivanov's top control turns every round into a long, slow squeeze.",
                bio: "Doesn't talk much. Doesn't need to. The mat says everything for him.", lossLine: "...Theory holds. Noted. Recalculating.", winLine: "Theory disproven. Again.")
        };
    }

    static GymInfo BuildBjjGym()
    {
        var trainerMoves = MoveDatabase.BjjTrainerMoves;
        var leaderMoves = MoveDatabase.BjjLeaderMoves;

        return new GymInfo
        {
            GymId = "bjj_gym",
            GymName = "Brazilian Jiu-Jitsu Academy",
            GymType = GymType.BrazilianJiuJitsu,
            Description = "The gentle art. Leverage and patience end fights on the ground.",
            Motto = "Patience is the deadliest weapon.",
            History = "A converted dance studio that became a proving ground for fighters who understand that the floor is not a place to fear.",
            RequiredGymId = "wrestling_gym",
            UnlockMoveId = MoveDatabase.RearNakedChoke.Id,
            Trainers = new List<OpponentInfo>
            {
                CreateOpponent("bjj_trainer_1", "Trainer Renzo", trainerMoves, health: 170, stamina: 62, strength: 14, defense: 14, speed: 13, striking: 15, grappling: 16, submission: 17, rewardXP: 60, rewardCoins: 32,
                    nickname: "The Lock", quote: "Tap or nap. Your call. Most pick nap by accident.", description: "Renzo hunts for the joint lock from the moment the fight hits the ground.",
                    bio: "Falls asleep mid-conversation if it's boring. Never mid-armbar.", lossLine: "Tapped. Properly. Respect where it's due.", winLine: "Sweet dreams. I'll be here when you wake up, still right."),
                CreateOpponent("bjj_trainer_2", "Trainer Helio", trainerMoves, health: 180, stamina: 64, strength: 15, defense: 15, speed: 14, striking: 16, grappling: 17, submission: 18, rewardXP: 65, rewardCoins: 35,
                    nickname: "Featherweight", quote: "Size is just a number until I'm on your back counting it for you.", description: "Helio uses leverage to make bigger fighters disappear.",
                    bio: "Smallest fighter in the gym. Most dangerous on the ground.", lossLine: "Size mattered. Once. Tell no one.", winLine: "Small fighter. Big back. Do the math."),
                CreateOpponent("bjj_trainer_3", "Trainer Carlos", trainerMoves, health: 190, stamina: 66, strength: 16, defense: 16, speed: 14, striking: 17, grappling: 18, submission: 19, rewardXP: 70, rewardCoins: 38,
                    nickname: "The Professor's Shadow", quote: "I've drilled this position ten thousand times. You're about to be the ten-thousand-and-first lesson.", description: "Carlos drills fundamentals until they become inevitabilities.",
                    bio: "Drills the same sweep ten thousand times. Calls it 'getting started.'", lossLine: "The student becomes the lesson. Noted in my own notebook.", winLine: "Class dismissed. Same time next week?")
            },
            Leader = CreateOpponent("bjj_leader", "Professor Silva", leaderMoves, health: 220, stamina: 80, strength: 18, defense: 18, speed: 16, striking: 19, grappling: 21, submission: 24, rewardXP: 150, rewardCoins: 75,
                nickname: "The Professor", quote: "Strength fades, fighters. Technique doesn't. Class is in session.", description: "Silva has never needed to win a striking exchange to win a fight.",
                bio: "Has taught everyone in this gym something. Tonight, the lesson runs the other way.", lossLine: "...Technique fades too, apparently. Add that to the syllabus.", winLine: "Strength fades. Technique doesn't. I told you. I always tell you.")
        };
    }

    static GymInfo BuildChampionshipGym()
    {
        var trainerMoves = MoveDatabase.ChampionshipTrainerMoves;
        var leaderMoves = MoveDatabase.ChampionshipLeaderMoves;

        return new GymInfo
        {
            GymId = "championship_gym",
            GymName = "Championship Gym",
            GymType = GymType.Championship,
            Description = "Every discipline, one cage. Only complete fighters leave as champions.",
            Motto = "Every discipline. One throne.",
            History = "Reserved for fighters who've already proven themselves elsewhere, the Championship Gym exists to test whether they can do it all at once.",
            RequiredGymId = "bjj_gym",
            UnlockMoveId = MoveDatabase.ElbowBarrage.Id,
            Trainers = new List<OpponentInfo>
            {
                CreateOpponent("championship_trainer_1", "Elite Trainer Kane", trainerMoves, health: 210, stamina: 75, strength: 18, defense: 18, speed: 17, striking: 20, grappling: 20, submission: 20, rewardXP: 90, rewardCoins: 45,
                    nickname: "Complete", quote: "I studied every discipline in this league. I don't have a weakness. That's not bragging, it's a warning.", description: "Kane blends every discipline into one suffocating gameplan.",
                    bio: "Studied every discipline in the league. Mastered most of them.", lossLine: "...A gap in the gameplan. First one. Noted, recorded, fixed by tomorrow.", winLine: "Complete fighters don't have bad nights. Tonight proved it."),
                CreateOpponent("championship_trainer_2", "Elite Trainer Mateo", trainerMoves, health: 220, stamina: 78, strength: 19, defense: 19, speed: 18, striking: 21, grappling: 21, submission: 21, rewardXP: 95, rewardCoins: 48,
                    nickname: "The Gatekeeper", quote: "Get past me, then we'll talk about you being champion material. Until then, sit down.", description: "Mateo exists to filter out anyone who isn't ready.",
                    bio: "Has personally ended more title runs than anyone admits.", lossLine: "...Through the gate. Fine. Wasn't expecting that. Wasn't NOT expecting it either.", winLine: "Gate's still shut. Come back when you've got a key."),
                CreateOpponent("championship_trainer_3", "Elite Trainer Yuki", trainerMoves, health: 230, stamina: 80, strength: 20, defense: 20, speed: 19, striking: 22, grappling: 22, submission: 22, rewardXP: 100, rewardCoins: 50,
                    nickname: "No Wasted Motion", quote: "Everything I do has a reason. You're about to find out what your reason was.", description: "Yuki's efficiency makes every exchange feel inevitable.",
                    bio: "Trains in total silence. Says noise is wasted motion.", lossLine: "...Unexpected. Recalculating. That's never happened before.", winLine: "No wasted motion. No wasted time. No surprise.")
            },
            Leader = CreateOpponent("championship_leader", "Champion Volkov", leaderMoves, health: 260, stamina: 95, strength: 23, defense: 22, speed: 21, striking: 25, grappling: 25, submission: 25, rewardXP: 250, rewardCoins: 120,
                nickname: "The Apex", quote: "Every fighter who's stood across from me thought they were the exception. I've got a long memory and an undefeated streak. Convince me you're different. I'll wait.",
                description: "Volkov has beaten every style the league has thrown at him. You're not the exception.",
                bio: "Champion Volkov has held the belt longer than some gyms have existed. Every discipline the league has, he's already beaten - and he remembers the name of every fighter who tried. Tonight, he's still waiting for a name worth remembering.",
                lossLine: "...Huh. There it is. The version where you're better than me. Didn't think I'd see it. Wear the belt better than I did - and remember MY name too.",
                winLine: "There's no version of tonight where you walk out of here better than me. Go train. Come back when there is one. I'll still be here.")
        };
    }
}
