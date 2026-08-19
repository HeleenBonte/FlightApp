# FlightApp (Zephyrus Airlines)

Een e-commerceplatform voor het boeken van vliegtuigtickets tussen zeven wereldsteden 
(New York, Londen, Tokio, Dubai, Sydney, Kaapstad, Singapore), ontwikkeld als schoolopdracht 
tijdens mijn Bachelor Toegepaste Informatica (VIVES). Project uitgevoerd met 
Bradley Sander.

## Technologieën
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core met MS SQL Server (gehost op Azure)
- Azure Key Vault voor het veilig beheren van API-sleutels en connectiegegevens
- HTML5, CSS3, jQuery, AJAX
- AutoMapper
- Layered architecture met Dependency Injection
- REST API's, getest via Postman en Swagger

## Architectuur
Het project is opgebouwd volgens een gelaagde architectuur:
- **FlightApp.Domains** — databasecontext en entiteiten
- **FlightApp.Repositories** — data access layer (DAO-pattern)
- **FlightApp.Services** — business logica
- **FlightApp.Util** — herbruikbare diensten (e-mail, PDF, hotel-API)
- **FlightApp** — MVC-project met controllers, views en viewmodels

## Functionaliteiten
- Zoeken en boeken van vluchten tussen 7 wereldsteden, met automatische overstapberekening
- Keuze tussen Economy- en Business-class, met automatische stoeltoewijzing
- Maaltijdkeuze tijdens het boekingsproces (standaard, vegetarisch, veganistisch, halal, 
  kosher, glutenvrij, en lokaal geïnspireerd naargelang de bestemming)
- Seizoensgebonden prijszetting (bv. hogere prijzen rond Kerstmis en tijdens de zomervakantie 
  op specifieke routes)
- Winkelmandje voor het beheren van geselecteerde vluchten en services
- Verplichte accountregistratie, met wachtwoordherstel en e-mailbevestiging
- Automatische bevestigingsmail met PDF-ticket en QR-code na boeking
- Overzicht van boekingsgeschiedenis met status per boeking, inclusief kosteloos annuleren 
  tot 7 dagen voor vertrek
- Hotelreservering bij de eindbestemming via integratie met de Booking.com API (met een 
  dummy-data fallback, nodig doordat het toegestane aantal API-calls kort voor de deadline 
  door de provider werd beperkt)
- Meertalige homepagina
- Eigen REST API's voor luchthavens, vluchten en gebruikers, getest via Postman en Swagger

## Mijn bijdrage
Dit project werd in duo ontwikkeld. Ik stond in voor het opzetten van de database en de 
volledige hosting op Azure, inclusief Azure Key Vault voor het veilig beheren van API-sleutels 
en connectiegegevens. Daarnaast bouwde ik de kernfunctionaliteit rond het zoeken en boeken van 
vluchten, het volledige authenticatiesysteem (login, registratie, wachtwoordherstel, 
e-mailbevestiging), en het boekingsproces inclusief maaltijdkeuze, stoeltoewijzing, en 
QR-code- en PDF-generatie voor tickets. Ik implementeerde ook de integratie met de Booking.com 
API voor hotelreserveringen en de REST API-endpoints van het platform. Mijn collega Bradley 
Sander werkte vooral aan de shoppingcart-functionaliteit, het passagiersformulier, de 
maaltijdkeuze-interface en en nam het voortouw in de algemene frontend-styling van de applicatie.
