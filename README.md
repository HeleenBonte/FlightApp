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

## Screenshots
<img width="945" height="508" alt="image" src="https://github.com/user-attachments/assets/cb3118e7-0222-4c9b-ae91-1c69daaee0ab" />
<img width="945" height="464" alt="image" src="https://github.com/user-attachments/assets/82c5a8bc-4a5c-4e85-8d45-489f8e4707ed" />
<img width="945" height="464" alt="image" src="https://github.com/user-attachments/assets/801e27e0-44a1-48d6-9676-c2782421f422" />
<img width="945" height="1252" alt="image" src="https://github.com/user-attachments/assets/e6b58b3a-4e6c-4ddc-bafc-f8bacd2af860" />
<img width="945" height="492" alt="image" src="https://github.com/user-attachments/assets/8304c192-d641-4017-9c0b-067a847a4561" />
<img width="945" height="508" alt="image" src="https://github.com/user-attachments/assets/f647aa56-0a79-4c62-b7ee-359b780bf6fd" />
<img width="945" height="634" alt="image" src="https://github.com/user-attachments/assets/f9ddae4e-9edb-429d-b05e-155038a630e1" />
<img width="945" height="969" alt="image" src="https://github.com/user-attachments/assets/12ed68ca-5d28-4720-8c45-38f87c9d626a" />
<img width="945" height="972" alt="image" src="https://github.com/user-attachments/assets/99b5424f-715e-4bf9-9ee9-87ef4fd1eeaf" />
<img width="945" height="463" alt="image" src="https://github.com/user-attachments/assets/76f1e6e9-9608-416d-8d04-673b47889840" />




