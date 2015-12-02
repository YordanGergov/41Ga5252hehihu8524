namespace TexasHoldem.AI.SmartPlayer.Helpers
{
    using TexasHoldem.Logic.Cards;

    public static class HandStrengthValuation
    {
        private const int MaxCardTypeValue = 14;

        private static readonly int[,] InPositionHandStrenght =
           {
                { 5, 5, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // AA AKs AQs AJs ATs A9s A8s A7s A6s A5s A4s A3s A2s
                { 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // AKo KK KQs KJs KTs K9s K8s K7s K6s K5s K4s K3s K2s
                { 4, 3, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // AQo KQo QQ QJs QTs Q9s Q8s Q7s Q6s Q5s Q4s Q3s Q2s
                { 3, 3, 3, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // AJo KJo QJo JJ JTs J9s J8s J7s J6s J5s J4s J3s J2s
                { 3, 3, 3, 3, 5, 3, 3, 3, 3, 3, 3, 3, 3  }, // ATo KTo QTo JTo TT T9s T8s T7s T6s T5s T4s T3s T2s
                { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // A9o K9o Q9o J9o T9o 99 98s 97s 96s 95s 94s 93s 92s
                { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // A8o K8o Q8o J8o T8o 98o 88 87s 86s 85s 84s 83s 82s
                { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // A7o K7o Q7o J7o T7o 97o 87o 77 76s 75s 74s 73s 72s
                { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // A6o K6o Q6o J6o T6o 96o 86o 76o 66 65s 64s 63s 62s
                { 3, 3, 1, 1, 1, 1, 1, 3, 3, 3, 3, 3, 3  }, // A5o K5o Q5o J5o T5o 95o 85o 75o 65o 55 54s 53s 52s
                { 3, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 3, 3  }, // A4o K4o Q4o J4o T4o 94o 84o 74o 64o 54o 44 43s 42s
                { 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3  }, // A3o K3o Q3o J3o T3o 93o 83o 73o 63o 53o 43o 33 32s
                { 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3  } // A2o K2o Q2o J2o T2o 92o 82o 72o 62o 52o 42o 32o 22
            };
        private static readonly int[,] OutOfPositionHandStrenght =
           {
                {5, 5, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3  }, // AA AKs AQs AJs ATs A9s A8s A7s A6s A5s A4s A3s A2s
                {5, 5, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0  }, // AKo KK KQs KJs KTs K9s K8s K7s K6s K5s K4s K3s K2s
                {4, 3, 5, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0  }, // AQo KQo QQ QJs QTs Q9s Q8s Q7s Q6s Q5s Q4s Q3s Q2s
                {3, 3, 3, 5, 3, 3, 0, 0, 0, 0, 0, 0, 0  }, // AJo KJo QJo JJ JTs J9s J8s J7s J6s J5s J4s J3s J2s
                {3, 3, 3, 3, 5, 3, 0, 0, 0, 0, 0, 0, 0  }, // ATo KTo QTo JTo TT T9s T8s T7s T6s T5s T4s T3s T2s
                { 3, 2, 2, 2, 3, 3, 3, 0, 0, 0, 0, 0, 0  }, // A9o K9o Q9o J9o T9o 99 98s 97s 96s 95s 94s 93s 92s
                { 3, 2, 2, 2, 2, 2, 3, 3, 0, 0, 0, 0, 0  }, // A8o K8o Q8o J8o T8o 98o 88 87s 86s 85s 84s 83s 82s
                { 2, 2, 2, 2, 2, 2, 2, 3, 3, 0, 0, 0, 0  }, // A7o K7o Q7o J7o T7o 97o 87o 77 76s 75s 74s 73s 72s
                { 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 0, 0, 0  }, // A6o K6o Q6o J6o T6o 96o 86o 76o 66 65s 64s 63s 62s
                { 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 0, 0, 0  }, // A5o K5o Q5o J5o T5o 95o 85o 75o 65o 55 54s 53s 52s
                { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 0, 0  }, // A4o K4o Q4o J4o T4o 94o 84o 74o 64o 54o 44 43s 42s
                { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 0  }, // A3o K3o Q3o J3o T3o 93o 83o 73o 63o 53o 43o 33 32s
                { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3  } // A2o K2o Q2o J2o T2o 92o 82o 72o 62o 52o 42o 32o 22
            };
        //Niki reccomendations
       // private static readonly int[,] StartingHandRecommendations =
       //     {
       //         { 3, 3, 3, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1 }, // AA AKs AQs AJs ATs A9s A8s A7s A6s A5s A4s A3s A2s
       //         { 3, 3, 3, 3, 3, 2, 1, 1, 1, 1, 1, 1, 1 }, // AKo KK KQs KJs KTs K9s K8s K7s K6s K5s K4s K3s K2s
       //         { 3, 3, 3, 3, 3, 2, 2, 0, 0, 0, 0, 0, 0 }, // AQo KQo QQ QJs QTs Q9s Q8s Q7s Q6s Q5s Q4s Q3s Q2s
       //         { 3, 3, 2, 3, 3, 3, 2, 1, 0, 0, 0, 0, 0 }, // AJo KJo QJo JJ JTs J9s J8s J7s J6s J5s J4s J3s J2s
       //         { 3, 2, 2, 2, 3, 3, 2, 1, 0, 0, 0, 0, 0 }, // ATo KTo QTo JTo TT T9s T8s T7s T6s T5s T4s T3s T2s
       //         { 1, 1, 1, 1, 1, 3, 2, 1, 1, 0, 0, 0, 0 }, // A9o K9o Q9o J9o T9o 99 98s 97s 96s 95s 94s 93s 92s
       //         { 1, 0, 0, 1, 1, 1, 3, 1, 1, 0, 0, 0, 0 }, // A8o K8o Q8o J8o T8o 98o 88 87s 86s 85s 84s 83s 82s
       //         { 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 0, 0, 0 }, // A7o K7o Q7o J7o T7o 97o 87o 77 76s 75s 74s 73s 72s
       //         { 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0 }, // A6o K6o Q6o J6o T6o 96o 86o 76o 66 65s 64s 63s 62s
       //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0, 0 }, // A5o K5o Q5o J5o T5o 95o 85o 75o 65o 55 54s 53s 52s
       //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 }, // A4o K4o Q4o J4o T4o 94o 84o 74o 64o 54o 44 43s 42s
       //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 }, // A3o K3o Q3o J3o T3o 93o 83o 73o 63o 53o 43o 33 32s
       //         { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 } // A2o K2o Q2o J2o T2o 92o 82o 72o 62o 52o 42o 32o 22
       //     };
        private static readonly double[,] NashEquilibriumPusher =
          {
                { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 }, // AA AKs AQs AJs ATs A9s A8s A7s A6s A5s A4s A3s A2s
                { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 19.9, 19.3 }, // AKo KK KQs KJs KTs K9s K8s K7s K6s K5s K4s K3s K2s
                { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 16.3, 13.5, 12.7 }, // AQo KQo QQ QJs QTs Q9s Q8s Q7s Q6s Q5s Q4s Q3s Q2s
                { 20, 20, 20, 20, 20, 20, 20, 20, 18.6, 14.7, 13.5, 10.6, 8.5 }, // AJo KJo QJo JJ JTs J9s J8s J7s J6s J5s J4s J3s J2s
                { 20, 20, 20, 20, 20, 20, 20, 20, 20, 11.9, 10.5, 7.7, 6.5 }, // ATo KTo QTo JTo TT T9s T8s T7s T6s T5s T4s T3s T2s
                { 20, 20, 20, 20, 20, 20, 20, 20, 20, 14.4, 6.9, 4.9, 3.7 }, // A9o K9o Q9o J9o T9o 99 98s 97s 96s 95s 94s 93s 92s
                { 20, 18.0, 13.0, 13.3, 17.5, 20, 20, 20, 20, 18.8, 10.1, 2.7, 2.5 }, // A8o K8o Q8o J8o T8o 98o 88 87s 86s 85s 84s 83s 82s
                { 20, 16.1, 10.3, 8.5, 9.0, 10.8, 14.7, 20, 20, 20, 13.9, 2.5, 2.1 }, // A7o K7o Q7o J7o T7o 97o 87o 77 76s 75s 74s 73s 72s
                { 20, 15.1, 9.6, 6.5, 5.7, 5.2, 7.0, 10.7, 20, 20, 16.3, 2.5, 2.0 }, // A6o K6o Q6o J6o T6o 96o 86o 76o 66 65s 64s 63s 62s
                { 20, 14.2, 8.9, 6.0, 4.1, 3.5, 3.0, 2.6, 2.4, 20, 20, 2.3, 2.0 }, // A5o K5o Q5o J5o T5o 95o 85o 75o 65o 55 54s 53s 52s
                { 20, 13.1, 7.9, 5.4, 3.8, 2.7, 2.3, 2.1, 2.0, 2.1, 20, 2.5, 1.8 }, // A4o K4o Q4o J4o T4o 94o 84o 74o 64o 54o 44 43s 42s
                { 20, 12.2, 7.5, 5.0, 3.4, 2.5, 1.9, 1.8, 1.7, 1.8, 1.6, 20, 1.7 }, // A3o K3o Q3o J3o T3o 93o 83o 73o 63o 53o 43o 33 32s
                { 20, 11.6, 7.0, 4.6, 2.9, 2.2, 1.8, 1.6, 1.5, 1.5, 1.4, 1.4, 20 } // A2o K2o Q2o J2o T2o 92o 82o 72o 62o 52o 42o 32o 22
            };
        private static readonly double[,] NashEquilibriumCaller =
        {
                { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 }, // AA AKs AQs AJs ATs A9s A8s A7s A6s A5s A4s A3s A2s
                { 20, 20, 20, 20, 20, 20, 17.6, 15.2, 14.3, 13.2, 12.1, 11.4, 10.7 }, // AKo KK KQs KJs KTs K9s K8s K7s K6s K5s K4s K3s K2s
                { 20, 20, 20, 20, 20, 16.1, 13.0, 10.5, 9.9, 8.9, 8.4, 7.8, 7.2 }, // AQo KQo QQ QJs QTs Q9s Q8s Q7s Q6s Q5s Q4s Q3s Q2s
                { 20, 20, 19.5, 20, 18.0, 13.4, 10.6, 8.8, 7.0, 6.9, 6.1, 5.8, 5.6 }, // AJo KJo QJo JJ JTs J9s J8s J7s J6s J5s J4s J3s J2s
                { 20, 20, 15.3, 12.7, 20, 11.5, 9.3, 7.4, 6.3, 5.2, 5.2, 4.8, 4.5 }, // ATo KTo QTo JTo TT T9s T8s T7s T6s T5s T4s T3s T2s
                { 20, 17.1, 11.7, 9.5, 8.4, 20, 8.2, 7.0, 5.8, 5.0, 4.3, 4.1, 3.9 }, // A9o K9o Q9o J9o T9o 99 98s 97s 96s 95s 94s 93s 92s
                { 20, 13.8, 9.7, 7.6, 6.6, 6.0, 20, 6.5, 5.6, 4.8, 4.1, 3.6, 3.5 }, // A8o K8o Q8o J8o T8o 98o 88 87s 86s 85s 84s 83s 82s
                { 20, 12.4, 8.0, 6.4, 5.5, 5.0, 4.7, 20, 5.4, 4.8, 4.1, 3.6, 3.3 }, // A7o K7o Q7o J7o T7o 97o 87o 77 76s 75s 74s 73s 72s
                { 20, 11.0, 7.3, 5.4, 4.6, 4.2, 4.1, 4.0, 20, 4.9, 4.1, 3.8, 3.3 }, // A6o K6o Q6o J6o T6o 96o 86o 76o 66 65s 64s 63s 62s
                { 20, 10.2, 6.8, 5.1, 4.0, 3.7, 3.6, 3.6, 3.7, 20, 4.6, 4.0, 4.6 }, // A5o K5o Q5o J5o T5o 95o 85o 75o 65o 55 54s 53s 52s
                { 18.3, 9.1, 6.2, 4.7, 3.8, 3.3, 3.2, 3.2, 3.3, 3.5, 20, 3.8, 3.4 }, // A4o K4o Q4o J4o T4o 94o 84o 74o 64o 54o 44 43s 42s
                { 16.6, 8.7, 5.9, 4.5, 3.6, 3.1, 2.9, 2.9, 2.9, 3.1, 3.0, 20, 3.3 }, // A3o K3o Q3o J3o T3o 93o 83o 73o 63o 53o 43o 33 32s
                { 15.8, 8.1, 5.6, 4.2, 3.5, 3.0, 2.8, 2.6, 2.7, 2.8, 2.7, 2.6, 15 } // A2o K2o Q2o J2o T2o 92o 82o 72o 62o 52o 42o 32o 22
            };

        // http://www.rakebackpros.net/texas-holdem-starting-hands/
        public static CardValuationType PreFlop(Card firstCard, Card secondCard, bool hasTheButton)
        {
            var value = 0;
            if (hasTheButton)
            {
                value = firstCard.Suit == secondCard.Suit
                 ? (firstCard.Type > secondCard.Type
                        ? InPositionHandStrenght[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]
                        : InPositionHandStrenght[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type])
                 : (firstCard.Type > secondCard.Type
                        ? InPositionHandStrenght[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type]
                        : InPositionHandStrenght[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]);
            }
            else {
                 value = firstCard.Suit == secondCard.Suit
                 ? (firstCard.Type > secondCard.Type
                        ? OutOfPositionHandStrenght[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]
                        : OutOfPositionHandStrenght[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type])
                 : (firstCard.Type > secondCard.Type
                        ? OutOfPositionHandStrenght[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type]
                        : OutOfPositionHandStrenght[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]);
            }
            //var value = firstCard.Suit == secondCard.Suit
            //              ? (firstCard.Type > secondCard.Type
            //                     ? StartingHandRecommendations[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]
            //                     : StartingHandRecommendations[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type])
            //              : (firstCard.Type > secondCard.Type
            //                     ? StartingHandRecommendations[MaxCardTypeValue - (int)secondCard.Type, MaxCardTypeValue - (int)firstCard.Type]
            //                     : StartingHandRecommendations[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type]);

            switch (value)
            {
                case 0:
                    return CardValuationType.Unplayable;
                case 1:
                    return CardValuationType.NotRecommended;
                case 2:
                    return CardValuationType.Risky;
                case 3:
                    return CardValuationType.Recommended;
                case 4:
                    return CardValuationType.Premium;
                default:
                    return CardValuationType.Unplayable;
            }
        }
        public static double GetPusherBlindsCount(Card firstCard, Card secondCard)
        {
            return NashEquilibriumPusher[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type];
        }

        public static double GetCallerBlindsCount(Card firstCard, Card secondCard)
        {
            return NashEquilibriumCaller[MaxCardTypeValue - (int)firstCard.Type, MaxCardTypeValue - (int)secondCard.Type];
        }
    }
}
