namespace TexasHoldem.AI.SmartPlayer
{
    using System;
    using System.Collections.Generic;
    using TexasHoldem.AI.SmartPlayer.Helpers;
    using TexasHoldem.Logic;
    using TexasHoldem.Logic.Extensions;
    using Logic.Helpers;
    using TexasHoldem.Logic.Players;
    using Logic.Cards;
    using System.Linq;
    using AI.Helpers;

    public class GoshoTheBot : BasePlayer
    {
        private static readonly IHandEvaluator HandEvaluator = new HandEvaluator();

        public static int mFactor;

        public static bool hasTheButton = false;

        public static bool iniative = false;

        public static bool is3bettedPot = false;

        public static bool hasTopPremium = false;


        public override string Name { get; } = "Gosho" + Guid.NewGuid();

        public override void StartGame(StartGameContext context)
        {            
            base.StartGame(context);
        }

        public override void StartHand(StartHandContext context)
        {
           
            is3bettedPot = false;
            hasTheButton = false;
            hasTopPremium = false;
            mFactor = context.MoneyLeft / (context.SmallBlind * 2);
            base.StartHand(context);
        }

        public override void StartRound(StartRoundContext context)
        {            
            base.StartRound(context);
        }
        public override PlayerAction GetTurn(GetTurnContext context)
        {
        
            #region PreFlopLogic            
            if (context.RoundType == GameRoundType.PreFlop)
            {
                //checks if we have the button
                if (context.MyMoneyInTheRound == context.SmallBlind)
                {
                    hasTheButton = true;
                }
                //checks if 3bettedPot; If the pot is big it is!
                if (context.CurrentPot > (context.SmallBlind*4))
                {
                    is3bettedPot = true;
                }

                var playHand = HandStrengthValuation.PreFlop(this.FirstCard, this.SecondCard, hasTheButton);

               
                //http://www.holdemresources.net/h/poker-theory/hune/usage.html

                #region mFactor< 20
                if (mFactor < 20)
                {

                    double nashEquillibriumRatio = 0;

                    if (hasTheButton)
                    {
                        nashEquillibriumRatio = HandStrengthValuation.GetPusherBlindsCount(this.FirstCard, this.SecondCard);
                    }
                    else
                    {
                        nashEquillibriumRatio = HandStrengthValuation.GetCallerBlindsCount(this.FirstCard, this.SecondCard);
                    }

                    //find if we have less money then the effective stack size
                    bool push = context.MoneyLeft <= (nashEquillibriumRatio * (context.SmallBlind* 2));

                    if (push)
                    {
                        return PlayerAction.Raise(context.CurrentMaxBet);
                    }
                    else
                    {
                        if (context.CanCheck)
                        {
                            return PlayerAction.CheckOrCall();
                        }
                        else
                        {
                            return PlayerAction.Fold();
                        }
                    }
                }
                #endregion

                #region UnplayableHands
                if (playHand == CardValuationType.Unplayable)
                {
                    if (context.CanCheck)
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else
                    {
                        return PlayerAction.Fold();
                    }
                }
                #endregion

                // raises risky hands only if in position and if it is not a 3betted Pot
                if (playHand == CardValuationType.Risky && context.MyMoneyInTheRound == context.SmallBlind)
                {
                 //   var smallBlindsTimes = RandomProvider.Next(1, 8);
                    return PlayerAction.Raise(context.SmallBlind * 6);
                }

                // folds if not in position
                else if(playHand == CardValuationType.Risky && !hasTheButton)
                {
                    
                    if (context.CanCheck)
                    {
                        return PlayerAction.CheckOrCall();
                    }
                    else
                    {
                        return PlayerAction.Fold();
                    }
                }
              
                // if recommended and not in 3bettedPot and a big M RAISES
                if (playHand == CardValuationType.Recommended)
                {
                    if (is3bettedPot && mFactor > 100)
                    {
                        return PlayerAction.CheckOrCall();
                    }

                    if (is3bettedPot && mFactor > 200)
                    {
                        return PlayerAction.Fold();
                    }
                    // var smallBlindsTimes = RandomProvider.Next(6, 10);
                    return PlayerAction.Raise(context.SmallBlind * 6);
                }
                //needs refactoring
                if (playHand == CardValuationType.Premium || playHand == CardValuationType.TopPremium)
                {
                    if (playHand == CardValuationType.TopPremium)
                    {
                        hasTopPremium = true;
                        return PlayerAction.Raise(context.SmallBlind * 10);
                    }

                    if (playHand == CardValuationType.Premium)
                    {
                        return PlayerAction.Raise(context.SmallBlind * 10);
                    }                    
                }

            // not sure if this doesn't brake everything
               if (context.CanCheck)
               {
                   return PlayerAction.CheckOrCall();
               }
             // not sure if this doesn't brake everything
                else
                {
                    return PlayerAction.Fold();
                }

            }
            #endregion

            
            #region Post-FlopLogic 
                  
            double currentPotRaise = context.CurrentPot * 0.55;
            int currentPotRaiseInt = (int)currentPotRaise;

            List<Card> allCards = new List<Card>(this.CommunityCards);
            allCards.Add(this.FirstCard);
            allCards.Add(this.SecondCard);
            HandRankType type = HandEvaluator.GetBestHand(allCards).RankType;
            var playerFirstHand = ParseHandToString.GenerateStringFromCard(this.FirstCard);
            var playerSecondHand = ParseHandToString.GenerateStringFromCard(this.SecondCard);

            string playerHand = playerFirstHand + " " + playerSecondHand;
            string openCards = string.Empty;

            foreach (var item in this.CommunityCards)
            {
                openCards += ParseHandToString.GenerateStringFromCard(item) + " ";
            }

            var chance = MonteCarloAnalysis.CalculateWinChance(playerHand, openCards.Trim());

            int Check = 15;

            int Raise = 60;

            int AllIn = 85;
            if (context.MoneyToCall <= (context.SmallBlind * 2))
            {
                return PlayerAction.CheckOrCall();
            }

            if (chance < Check)
            {
                return PlayerAction.Fold();
            }
            if (chance < Raise)
            {
                return PlayerAction.CheckOrCall();
            }
            else if (chance < AllIn)
            {
                if ((int)type >= (int)HandRankType.Pair)
                {
                    return PlayerAction.Raise(currentPotRaiseInt);
                }
                else
                {
                    return PlayerAction.Raise(context.SmallBlind * 4);
       
                }
            }
            else
            {
                return PlayerAction.Raise(context.CurrentMaxBet);
            }       
        }
        #endregion

    }
}
