using System;

namespace boin.Review
{
    // 银行卡审核
	public class BankCardReview: IReviewInterface
    {
		public BankCardReview()
		{
		}


        //6222801251011210972
        public static bool Chenk(string bankCard)
        {
            //如果小于15位或大于19位为假
            if (bankCard.Length < 15 || bankCard.Length > 19)
            {
                return false;
            }
            //声明一个bit 接收银行卡截取出来的字符串
            char bit = getBank(bankCard.Substring(0, bankCard.Length - 1));
            if (bit == 'F')
            {
                return false;//不是数据返回false
            }
            return bankCard[bankCard.Length - 1] == bit;
        }

        static char getBank(string non)
        {
            char[] cs = non.ToArray();
            foreach (var c in cs)
            {
                if (c < '0' || c > '9')
                {
                    return 'F'; //如果传的不是数据返回F
                }
            }

            int sum = 0;
            for (int i = cs.Length - 1, j = 0; i >= 0; i--, j++)
            {
                int k = cs[i] - '0';
                if (j % 2 == 0)
                {
                    k *= 2;
                    k = k / 10 + k % 10;
                }
                sum += k;
            }
            return (sum % 10 == 0) ? '0' : (char)((10 - sum) % 10 + '0');
        }

        public List<ReviewResult> Review(User user, Order order)
        {
            List<ReviewResult> rs = new List<ReviewResult>();
            var chenk = Chenk(order.CardNo);



            return rs;
        }

    }
}

