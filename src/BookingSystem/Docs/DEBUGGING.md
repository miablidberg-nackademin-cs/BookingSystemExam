# Debugging – Hantering av ogiltig inmatning av antal deltagare

## 1. Beskrivning av felet

Jag undersökte bland annat felet där programmet kraschar när användaren skriver in något som inte är ett heltal när antalet deltagare ska anges. Jag återskapade felet genom att testa att skriva in bokstäver istället för en siffra. Precis som det stod i uppgiften kraschade programmet, istället för att visa ett felmeddelande och låta användaren göra flera försök.

## 2. Orsak till felet

Problemet fanns i metoden `CreateBooking` i klassen `BookingConsole`, där antalet deltagare skulle läsas in. Där användes ett `Parse`-anrop för att omvandla användarens inmatning till en `int`.

Eftersom anropet gjordes direkt på det användaren skrev in, utan att först kontrollera om inmatningen var ett giltigt heltal, uppstod ett problem när användaren skrev in något annat. Om inmatningen inte kunde omvandlas till en `int` med `Parse` kraschade programmet eftersom ett undantag (*exception*) kastades av metoden.

## 3. Lösning

Jag ändrade koden till att istället anropa en ny metod, `ReadParticipants`, som innehåller en loop där användaren ombeds ange antalet deltagare tills ett giltigt heltal anges.

Istället för att använda `Parse` anropas `TryParse`, som inte kastar ett undantag om användaren skriver in något annat än ett heltal. `TryParse` returnerar istället ett bool-värde som visar om omvandlingen lyckades eller inte. Detta använder jag för att kontrollera om inmatningen är giltig.

Om användarens inmatning är ett heltal returnerar metoden det omvandlade talet, som sedan sätts till antalet deltagare för bokningen. Om användaren inte har skrivit in ett heltal visas ett felmeddelande som uppmanar användaren att ange antalet deltagare som ett heltal. Loopen fortsätter tills användaren har skrivit in ett giltigt heltal.

## 4. Alternativa lösningar

En annan lösning hade varit att helt enkelt byta ut `Parse` mot `TryParse`, anpassat efter hur `TryParse` ger tillbaka det översatta värdet, och låta servicen hantera resultatet om användaren skriver in något annat än ett heltal.

Det skulle också kunna fungera eftersom servicen kontrollerar om antalet deltagare är större än en person. Då resultatvariabeln i `int.TryParse` sätts till `0` om omvandlingen misslyckas, skulle ett felmeddelande ges genom servicen.

Den här lösningen skulle dock innebära att användaren får ett felmeddelande först efter att alla uppgifter har skrivits in. Användaren skulle då inte få möjlighet att korrigera inmatningen direkt, utan blir skickad tillbaka till huvudmenyn och får sedan börja om och fylla i alla uppgifter igen.

Jag anser att det är mer användarvänligt att låta användaren göra flera försök att ange antalet deltagare. Därför valde jag den lösning jag implementerade.

## 5. Testning

Efter att jag gjort ändringen startade jag programmet igen och testade att skriva in ord, bokstäver och decimaltal för att kontrollera att lösningen fungerade.

Alla felaktiga inmatningar jag provade hanterades korrekt. När jag angav ett giltigt heltal avslutades loopen som förväntat och heltalet sparades i egenskapen för antalet deltagare.

## 6. Förebyggande av framtida fel

Det finns just nu inget automatiserat test som skyddar mot att felet återkommer. Det kan testas manuellt genom att skriva in ogiltiga värden och kontrollera hur applikationen reagerar.

Ett annat alternativ skulle vara att skapa ett automatiserat test där användarens inmatning simuleras på något sätt, exempelvis genom att testa metoden med olika inmatningar. Jag är inte helt säker på exakt hur detta skulle implementeras, men jag tror att det finns sätt att testa funktionen automatiskt.
