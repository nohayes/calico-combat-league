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
        int rewardXP, int rewardCoins, string nickname = "", string quote = "", string description = "")
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
                CreateOpponent("boxing_trainer_1", "Trainer Rocco", moves, health: 80, stamina: 40, strength: 8, defense: 7, speed: 8, striking: 10, grappling: 5, submission: 4, rewardXP: 15, rewardCoins: 10,
                    nickname: "The Brick Wall", quote: "You want to get past me? Bring a truck.", description: "Immovable in the pocket, Rocco out-walls everyone who tries to out-punch him."),
                CreateOpponent("boxing_trainer_2", "Trainer Sal", moves, health: 90, stamina: 42, strength: 9, defense: 8, speed: 9, striking: 11, grappling: 6, submission: 5, rewardXP: 20, rewardCoins: 12,
                    nickname: "Quick Hands", quote: "Speed kills. I should know.", description: "Sal throws combinations faster than most fighters can think."),
                CreateOpponent("boxing_trainer_3", "Trainer Mickey", moves, health: 100, stamina: 45, strength: 10, defense: 9, speed: 9, striking: 12, grappling: 6, submission: 5, rewardXP: 25, rewardCoins: 15,
                    nickname: "Iron Jaw", quote: "Hit me. I'll wait.", description: "Mickey's chin has never been the same fighter's problem twice.")
            },
            Leader = CreateOpponent("boxing_leader", "Gym Leader Diaz", moves, health: 130, stamina: 55, strength: 12, defense: 11, speed: 11, striking: 15, grappling: 7, submission: 6, rewardXP: 60, rewardCoins: 30,
                nickname: "The Closer", quote: "Round one is just the introduction.", description: "Diaz doesn't chase knockouts. He waits for the mistake that gives him one.")
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
                    nickname: "Whip Leg", quote: "My leg remembers every kick yours forgot to block.", description: "Niran's low kicks have ended more fights than his hands ever will."),
                CreateOpponent("muaythai_trainer_2", "Trainer Somchai", trainerMoves, health: 120, stamina: 50, strength: 11, defense: 10, speed: 12, striking: 14, grappling: 8, submission: 6, rewardXP: 35, rewardCoins: 20,
                    nickname: "The Elbow", quote: "Get close. See what happens.", description: "Somchai turns the clinch into a minefield."),
                CreateOpponent("muaythai_trainer_3", "Trainer Anan", trainerMoves, health: 130, stamina: 52, strength: 12, defense: 11, speed: 12, striking: 15, grappling: 9, submission: 7, rewardXP: 40, rewardCoins: 22,
                    nickname: "Iron Shin", quote: "Check this if you can.", description: "Anan trades leg kicks like he's allergic to losing.")
            },
            Leader = CreateOpponent("muaythai_leader", "Gym Leader Sakda", leaderMoves, health: 160, stamina: 65, strength: 14, defense: 13, speed: 14, striking: 18, grappling: 10, submission: 8, rewardXP: 90, rewardCoins: 45,
                nickname: "The Storm", quote: "I don't fight in rounds. I fight in waves.", description: "Sakda overwhelms with volume until there's nothing left to defend.")
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
                    nickname: "The Anchor", quote: "Once I'm on you, gravity does the rest.", description: "Kurt's takedowns aren't flashy. They're inevitable."),
                CreateOpponent("wrestling_trainer_2", "Trainer Brock", trainerMoves, health: 150, stamina: 57, strength: 14, defense: 13, speed: 13, striking: 15, grappling: 15, submission: 9, rewardXP: 50, rewardCoins: 28,
                    nickname: "Big Country", quote: "I've carried heavier.", description: "Brock's strength advantage shows up the moment the fight hits the mat."),
                CreateOpponent("wrestling_trainer_3", "Trainer Danny", trainerMoves, health: 160, stamina: 60, strength: 15, defense: 14, speed: 13, striking: 16, grappling: 16, submission: 9, rewardXP: 55, rewardCoins: 30,
                    nickname: "Scramble", quote: "Stand still. I dare you.", description: "Danny never stops moving, never stops chaining.")
            },
            Leader = CreateOpponent("wrestling_leader", "Gym Leader Ivanov", leaderMoves, health: 190, stamina: 75, strength: 17, defense: 16, speed: 15, striking: 18, grappling: 20, submission: 10, rewardXP: 120, rewardCoins: 60,
                nickname: "The Vice", quote: "Escape is a theory. I disprove it.", description: "Ivanov's top control turns every round into a long, slow squeeze.")
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
                    nickname: "The Lock", quote: "Tap or nap, your choice.", description: "Renzo hunts for the joint lock from the moment the fight hits the ground."),
                CreateOpponent("bjj_trainer_2", "Trainer Helio", trainerMoves, health: 180, stamina: 64, strength: 15, defense: 15, speed: 14, striking: 16, grappling: 17, submission: 18, rewardXP: 65, rewardCoins: 35,
                    nickname: "Featherweight", quote: "Size is just a number until I'm on your back.", description: "Helio uses leverage to make bigger fighters disappear."),
                CreateOpponent("bjj_trainer_3", "Trainer Carlos", trainerMoves, health: 190, stamina: 66, strength: 16, defense: 16, speed: 14, striking: 17, grappling: 18, submission: 19, rewardXP: 70, rewardCoins: 38,
                    nickname: "The Professor's Shadow", quote: "I learned from the best. Now you learn from me.", description: "Carlos drills fundamentals until they become inevitabilities.")
            },
            Leader = CreateOpponent("bjj_leader", "Professor Silva", leaderMoves, health: 220, stamina: 80, strength: 18, defense: 18, speed: 16, striking: 19, grappling: 21, submission: 24, rewardXP: 150, rewardCoins: 75,
                nickname: "The Professor", quote: "Strength fades. Technique doesn't.", description: "Silva has never needed to win a striking exchange to win a fight.")
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
                    nickname: "Complete", quote: "I don't have a weakness. That's the point.", description: "Kane blends every discipline into one suffocating gameplan."),
                CreateOpponent("championship_trainer_2", "Elite Trainer Mateo", trainerMoves, health: 220, stamina: 78, strength: 19, defense: 19, speed: 18, striking: 21, grappling: 21, submission: 21, rewardXP: 95, rewardCoins: 48,
                    nickname: "The Gatekeeper", quote: "Get past me, then talk about being champion.", description: "Mateo exists to filter out anyone who isn't ready."),
                CreateOpponent("championship_trainer_3", "Elite Trainer Yuki", trainerMoves, health: 230, stamina: 80, strength: 20, defense: 20, speed: 19, striking: 22, grappling: 22, submission: 22, rewardXP: 100, rewardCoins: 50,
                    nickname: "No Wasted Motion", quote: "Everything I do has a reason.", description: "Yuki's efficiency makes every exchange feel inevitable.")
            },
            Leader = CreateOpponent("championship_leader", "Champion Volkov", leaderMoves, health: 260, stamina: 95, strength: 23, defense: 22, speed: 21, striking: 25, grappling: 25, submission: 25, rewardXP: 250, rewardCoins: 120,
                nickname: "The Apex", quote: "There's no version of this where you're better than me.", description: "Volkov has beaten every style the league has thrown at him. You're not the exception.")
        };
    }
}
