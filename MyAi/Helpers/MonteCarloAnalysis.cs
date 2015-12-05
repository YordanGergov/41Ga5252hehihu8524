namespace TexasHoldem
{
    using HoldemHand;

    public static class MonteCarloAnalysis
    {
        private const int HandCount = 1000;

        public static double CalculateWinChance(string playerCards, string communityCards)
        {
            ulong playerHandParsed = Hand.ParseHand(playerCards);    
            ulong communityCardsParsed = Hand.ParseHand(communityCards);
            int wins = 0;
            int ties = 0;
            int count = 0;
            foreach (ulong boardmask in Hand.RandomHands(communityCardsParsed, playerHandParsed, 5, HandCount))
            {
                ulong opponentMask = Hand.RandomHand(boardmask | playerHandParsed, 2);
                uint playerHand = Hand.Evaluate(playerHandParsed | boardmask, 7);
                uint opponentHand = Hand.Evaluate(opponentMask | boardmask, 7);
                if (playerHand > opponentHand)
                {
                    wins++;
                }
                else if (playerHand == opponentHand)
                {
                    ties++;
                }
                count++;
            }

            return (((double)wins) + ((double)ties) / 2.0) / ((double)count) * 100.0;
        } 
    }
}
