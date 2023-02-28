using System;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace boin.Review;

public class ReviewResult
{
    public static readonly ReadOnlyCollection<ReviewResult> Empty = new ReadOnlyCollection<ReviewResult>(new List<ReviewResult>());

    public int Code;
    public string Msg = string.Empty;
}

public interface IReviewOrder
{
    ReadOnlyCollection<ReviewResult> Review(Order order);
}

public interface IReviewUser
{
    ReadOnlyCollection<ReviewResult> Review(User user);
}

// 审核接口
public interface IReviewInterface: IReviewOrder, IReviewUser
{
}

