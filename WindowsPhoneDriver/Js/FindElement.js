var selector = FindElementFuntion("$0", "$1");

if (selector) {
    SeleniumWindowsPhoneDriver.SaveElement(selector);
    //SeleniumWindowsPhoneDriver.BuildWebElement(selector);
}
else {
    "";
}