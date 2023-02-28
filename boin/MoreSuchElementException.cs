﻿using System;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace boin;

public class MoreSuchElementException : Exception
{
	public By By { get; set; }
	public ReadOnlyCollection<IWebElement> Elemens { get; set; }

	public MoreSuchElementException(By by, string msg, ReadOnlyCollection<IWebElement> es) : base(msg)
	{
		Elemens = es;
		By = by;
	}
}

