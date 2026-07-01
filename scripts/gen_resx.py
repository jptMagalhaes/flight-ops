from pathlib import Path

keys_en = {
    "AppName": "FlightOps",
    "Nav.Home": "Home", "Nav.Airports": "Airports", "Nav.Aircrafts": "Aircrafts", "Nav.Flights": "Flights",
    "Nav.Report": "Report", "Nav.Simulation": "Simulation", "Nav.Language": "Language",
    "Lang.En": "English", "Lang.Pt": "Português", "Lang.De": "Deutsch",
    "Btn.Create": "Create", "Btn.CreateNew": "Create New", "Btn.Save": "Save", "Btn.Edit": "Edit",
    "Btn.Delete": "Delete", "Btn.BackToList": "Back to List", "Btn.BackToFlights": "Back to Flights",
    "Btn.StartSimulation": "Start Simulation", "Btn.Cancel": "Cancel",
    "Footer.Privacy": "Privacy", "Footer.Copyright": "All rights reserved.",
    "Title.Home": "Home", "Title.Privacy": "Privacy Policy", "Title.Airports": "Airports",
    "Title.CreateAirport": "Create Airport", "Title.EditAirport": "Edit Airport",
    "Title.Aircrafts": "Aircrafts", "Title.CreateAircraft": "Create Aircraft", "Title.EditAircraft": "Edit Aircraft",
    "Title.Flights": "Flights", "Title.CreateFlight": "Create Flight", "Title.EditFlight": "Edit Flight",
    "Title.FlightReport": "Flight Report", "Title.Simulation": "Flight Simulation", "Title.Error": "Error",
    "Col.Name": "Name", "Col.City": "City", "Col.Country": "Country", "Col.IATA": "IATA",
    "Col.Latitude": "Latitude", "Col.Longitude": "Longitude", "Col.Model": "Model",
    "Col.TakeOffEffort": "TakeOff Effort", "Col.FuelPerKm": "Fuel / km", "Col.CruiseSpeed": "Cruise Speed (km/h)",
    "Col.Origin": "Origin", "Col.Destination": "Destination", "Col.Aircraft": "Aircraft",
    "Col.Distance": "Distance (km)", "Col.Fuel": "Fuel", "Col.Departure": "Departure",
    "Col.Arrival": "Arrival", "Col.Status": "Status", "Col.Id": "Id", "Col.Actions": "Actions",
    "Origin": "Origin", "Destination": "Destination", "Aircraft": "Aircraft",
    "DepartureTime": "Departure Time", "ArrivalTime": "Arrival Time", "Status": "Status",
    "Name": "Name", "City": "City", "Country": "Country", "IATA": "IATA",
    "Latitude": "Latitude", "Longitude": "Longitude", "Model": "Model",
    "TakeOffEffort": "TakeOff Effort", "FuelConsumptionPerKm": "Fuel Consumption / km",
    "CruiseSpeedKmh": "Cruise Speed (km/h)",
    "Label.DistanceKm": "Distance (km)", "Label.Fuel": "Fuel", "Label.ArrivalTime": "Arrival Time",
    "Label.Locked": "Locked", "Label.Calculated": "Calculated",
    "FlightStatus.Scheduled": "Scheduled", "FlightStatus.Departed": "Departed",
    "FlightStatus.Arrived": "Arrived", "FlightStatus.Cancelled": "Cancelled",
    "Error.CreateFlightFailed": "Unable to create flight. Check airports and aircraft.",
    "Error.UpdateFlightFailed": "Unable to update flight.",
    "Error.CreateAirportFailed": "Failed to create airport.",
    "Error.UpdateAirportFailed": "Failed to update airport.",
    "Error.DeleteAirportReferenced": "Cannot delete: airport is referenced by flights.",
    "Error.CreateAircraftFailed": "Failed to create aircraft.",
    "Error.UpdateAircraftFailed": "Failed to update aircraft.",
    "Error.DeleteAircraftReferenced": "Cannot delete: aircraft is referenced by flights.",
    "Confirm.DeleteFlight": "Delete flight #{0}?",
    "Confirm.DeleteAirport": "Delete {0}?",
    "Confirm.DeleteAircraft": "Delete {0}?",
    "Confirm.CannotUndo": "This action cannot be undone.",
    "Empty.Flights.Title": "No flights yet",
    "Empty.Flights.Message": "Create your first flight to start tracking operations.",
    "Empty.Airports.Title": "No airports yet",
    "Empty.Airports.Message": "Add an airport to start building your network.",
    "Empty.Aircraft.Title": "No aircrafts yet",
    "Empty.Aircraft.Message": "Add a aircraft to start assigning flights.",
    "Alert.EditBlocked": "This flight cannot be edited because its status is {0}.",
    "Report.Summary": "Summary of all flights including calculated distance and fuel.",
    "Home.HeroTitle": "Flight Operations Center",
    "Home.HeroSubtitle": "Manage airports, fleet, and flights with real-time distance and fuel calculations.",
    "Home.Card.Airports": "Airports", "Home.Card.Aircrafts": "Fleet", "Home.Card.Flights": "Flights",
    "Home.Card.Report": "Reports", "Home.Card.Simulation": "Live Simulation",
    "Home.Card.AirportsDesc": "Manage airport registry and coordinates.",
    "Home.Card.AircraftsDesc": "Configure aircraft performance profiles.",
    "Home.Card.FlightsDesc": "Schedule and track flight operations.",
    "Home.Card.ReportDesc": "View operational summaries and metrics.",
    "Home.Card.SimulationDesc": "Watch flights traverse the globe in real time.",
    "Privacy.Intro": "This policy describes how FlightOps handles your data.",
    "Privacy.DataCollected": "Data We Collect",
    "Privacy.DataCollectedText": "FlightOps stores operational data you enter: airports, aircraft, and flight schedules. No personal user accounts are required.",
    "Privacy.Purpose": "Purpose",
    "Privacy.PurposeText": "Data is used solely to provide flight management and simulation features within this application.",
    "Privacy.Contact": "Contact",
    "Privacy.ContactText": "For privacy inquiries, contact the FlightOps administrator.",
    "Privacy.Cookies": "Cookies",
    "Privacy.CookiesText": "We use a culture preference cookie to remember your language selection.",
    "Simulation.SelectRoute": "Select a route to simulate",
    "Simulation.Start": "Start",
    "Simulation.Distance": "Distance",
    "Simulation.Duration": "Estimated duration",
    "Simulation.Instructions": "Choose origin and destination airports, then start the simulation.",
    "Error.PageTitle": "Error.",
    "Error.Message": "An error occurred while processing your request.",
    "Error.RequestId": "Request ID:",
    "Error.DevModeTitle": "Development Mode",
    "Error.DevModeText1": "Swapping to Development environment will display more detailed information about the error that occurred.",
    "Error.DevModeText2": "The Development environment should not be enabled for deployed applications.",
}

keys_pt = {
    "AppName": "FlightOps",
    "Nav.Home": "Início", "Nav.Airports": "Aeroportos", "Nav.Aircrafts": "Aviões", "Nav.Flights": "Voos",
    "Nav.Report": "Relatório", "Nav.Simulation": "Simulação", "Nav.Language": "Idioma",
    "Lang.En": "English", "Lang.Pt": "Português", "Lang.De": "Deutsch",
    "Btn.Create": "Criar", "Btn.CreateNew": "Criar Novo", "Btn.Save": "Guardar", "Btn.Edit": "Editar",
    "Btn.Delete": "Eliminar", "Btn.BackToList": "Voltar à Lista", "Btn.BackToFlights": "Voltar aos Voos",
    "Btn.StartSimulation": "Iniciar Simulação", "Btn.Cancel": "Cancelar",
    "Footer.Privacy": "Privacidade", "Footer.Copyright": "Todos os direitos reservados.",
    "Title.Home": "Início", "Title.Privacy": "Política de Privacidade", "Title.Airports": "Aeroportos",
    "Title.CreateAirport": "Criar Aeroporto", "Title.EditAirport": "Editar Aeroporto",
    "Title.Aircrafts": "Aviões", "Title.CreateAircraft": "Criar Avião", "Title.EditAircraft": "Editar Avião",
    "Title.Flights": "Voos", "Title.CreateFlight": "Criar Voo", "Title.EditFlight": "Editar Voo",
    "Title.FlightReport": "Relatório de Voos", "Title.Simulation": "Simulação de Voo", "Title.Error": "Erro",
    "Col.Name": "Nome", "Col.City": "Cidade", "Col.Country": "País", "Col.IATA": "IATA",
    "Col.Latitude": "Latitude", "Col.Longitude": "Longitude", "Col.Model": "Modelo",
    "Col.TakeOffEffort": "Esforço Descolagem", "Col.FuelPerKm": "Combustível / km", "Col.CruiseSpeed": "Velocidade Cruzeiro (km/h)",
    "Col.Origin": "Origem", "Col.Destination": "Destino", "Col.Aircraft": "Avião",
    "Col.Distance": "Distância (km)", "Col.Fuel": "Combustível", "Col.Departure": "Partida",
    "Col.Arrival": "Chegada", "Col.Status": "Estado", "Col.Id": "Id", "Col.Actions": "Ações",
    "Origin": "Origem", "Destination": "Destino", "Aircraft": "Avião",
    "DepartureTime": "Hora de Partida", "ArrivalTime": "Hora de Chegada", "Status": "Estado",
    "Name": "Nome", "City": "Cidade", "Country": "País", "IATA": "IATA",
    "Latitude": "Latitude", "Longitude": "Longitude", "Model": "Modelo",
    "TakeOffEffort": "Esforço de Descolagem", "FuelConsumptionPerKm": "Consumo Combustível / km",
    "CruiseSpeedKmh": "Velocidade de Cruzeiro (km/h)",
    "Label.DistanceKm": "Distância (km)", "Label.Fuel": "Combustível", "Label.ArrivalTime": "Hora de Chegada",
    "Label.Locked": "Bloqueado", "Label.Calculated": "Calculado",
    "FlightStatus.Scheduled": "Agendado", "FlightStatus.Departed": "Partiu",
    "FlightStatus.Arrived": "Chegou", "FlightStatus.Cancelled": "Cancelado",
    "Error.CreateFlightFailed": "Não foi possível criar o voo. Verifique aeroportos e avião.",
    "Error.UpdateFlightFailed": "Não foi possível atualizar o voo.",
    "Error.CreateAirportFailed": "Falha ao criar aeroporto.",
    "Error.UpdateAirportFailed": "Falha ao atualizar aeroporto.",
    "Error.DeleteAirportReferenced": "Não é possível eliminar: aeroporto referenciado por voos.",
    "Error.CreateAircraftFailed": "Falha ao criar avião.",
    "Error.UpdateAircraftFailed": "Falha ao atualizar avião.",
    "Error.DeleteAircraftReferenced": "Não é possível eliminar: avião referenciado por voos.",
    "Confirm.DeleteFlight": "Eliminar voo #{0}?",
    "Confirm.DeleteAirport": "Eliminar {0}?",
    "Confirm.DeleteAircraft": "Eliminar {0}?",
    "Confirm.CannotUndo": "Esta ação não pode ser desfeita.",
    "Empty.Flights.Title": "Ainda não há voos",
    "Empty.Flights.Message": "Crie o seu primeiro voo para começar a acompanhar as operações.",
    "Empty.Airports.Title": "Ainda não há aeroportos",
    "Empty.Airports.Message": "Adicione um aeroporto para começar a construir a sua rede.",
    "Empty.Aircraft.Title": "Ainda não há aviões",
    "Empty.Aircraft.Message": "Adicione um avião para começar a atribuir voos.",
    "Alert.EditBlocked": "Este voo não pode ser editado porque o estado é {0}.",
    "Report.Summary": "Resumo de todos os voos incluindo distância e combustível calculados.",
    "Home.HeroTitle": "Centro de Operações de Voo",
    "Home.HeroSubtitle": "Gira aeroportos, frota e voos com cálculos de distância e combustível em tempo real.",
    "Home.Card.Airports": "Aeroportos", "Home.Card.Aircrafts": "Frota", "Home.Card.Flights": "Voos",
    "Home.Card.Report": "Relatórios", "Home.Card.Simulation": "Simulação ao Vivo",
    "Home.Card.AirportsDesc": "Gerir registo de aeroportos e coordenadas.",
    "Home.Card.AircraftsDesc": "Configurar perfis de desempenho das aeronaves.",
    "Home.Card.FlightsDesc": "Agendar e acompanhar operações de voo.",
    "Home.Card.ReportDesc": "Ver resumos operacionais e métricas.",
    "Home.Card.SimulationDesc": "Veja voos atravessarem o globo em tempo real.",
    "Privacy.Intro": "Esta política descreve como o FlightOps trata os seus dados.",
    "Privacy.DataCollected": "Dados que Recolhemos",
    "Privacy.DataCollectedText": "O FlightOps armazena dados operacionais que introduz: aeroportos, aeronaves e horários de voo. Não são necessárias contas pessoais.",
    "Privacy.Purpose": "Finalidade",
    "Privacy.PurposeText": "Os dados são usados exclusivamente para funcionalidades de gestão e simulação de voos nesta aplicação.",
    "Privacy.Contact": "Contacto",
    "Privacy.ContactText": "Para questões de privacidade, contacte o administrador do FlightOps.",
    "Privacy.Cookies": "Cookies",
    "Privacy.CookiesText": "Utilizamos um cookie de preferência de cultura para memorizar o idioma selecionado.",
    "Simulation.SelectRoute": "Selecione uma rota para simular",
    "Simulation.Start": "Iniciar",
    "Simulation.Distance": "Distância",
    "Simulation.Duration": "Duração estimada",
    "Simulation.Instructions": "Escolha aeroportos de origem e destino, depois inicie a simulação.",
    "Error.PageTitle": "Erro.",
    "Error.Message": "Ocorreu um erro ao processar o seu pedido.",
    "Error.RequestId": "ID do Pedido:",
    "Error.DevModeTitle": "Modo de Desenvolvimento",
    "Error.DevModeText1": "Mudar para o ambiente Development mostrará informação detalhada sobre o erro.",
    "Error.DevModeText2": "O ambiente Development não deve estar ativo em aplicações publicadas.",
}

keys_de = {
    "AppName": "FlightOps",
    "Nav.Home": "Start", "Nav.Airports": "Flughäfen", "Nav.Aircrafts": "Flugzeuge", "Nav.Flights": "Flüge",
    "Nav.Report": "Bericht", "Nav.Simulation": "Simulation", "Nav.Language": "Sprache",
    "Lang.En": "English", "Lang.Pt": "Português", "Lang.De": "Deutsch",
    "Btn.Create": "Erstellen", "Btn.CreateNew": "Neu erstellen", "Btn.Save": "Speichern", "Btn.Edit": "Bearbeiten",
    "Btn.Delete": "Löschen", "Btn.BackToList": "Zurück zur Liste", "Btn.BackToFlights": "Zurück zu Flügen",
    "Btn.StartSimulation": "Simulation starten", "Btn.Cancel": "Abbrechen",
    "Footer.Privacy": "Datenschutz", "Footer.Copyright": "Alle Rechte vorbehalten.",
    "Title.Home": "Start", "Title.Privacy": "Datenschutzerklärung", "Title.Airports": "Flughäfen",
    "Title.CreateAirport": "Flughafen erstellen", "Title.EditAirport": "Flughafen bearbeiten",
    "Title.Aircrafts": "Flugzeuge", "Title.CreateAircraft": "Flugzeug erstellen", "Title.EditAircraft": "Flugzeug bearbeiten",
    "Title.Flights": "Flüge", "Title.CreateFlight": "Flug erstellen", "Title.EditFlight": "Flug bearbeiten",
    "Title.FlightReport": "Flugbericht", "Title.Simulation": "Flugsimulation", "Title.Error": "Fehler",
    "Col.Name": "Name", "Col.City": "Stadt", "Col.Country": "Land", "Col.IATA": "IATA",
    "Col.Latitude": "Breitengrad", "Col.Longitude": "Längengrad", "Col.Model": "Modell",
    "Col.TakeOffEffort": "Startaufwand", "Col.FuelPerKm": "Kraftstoff / km", "Col.CruiseSpeed": "Reisegeschwindigkeit (km/h)",
    "Col.Origin": "Abflug", "Col.Destination": "Ankunft", "Col.Aircraft": "Flugzeug",
    "Col.Distance": "Entfernung (km)", "Col.Fuel": "Kraftstoff", "Col.Departure": "Abflug",
    "Col.Arrival": "Ankunft", "Col.Status": "Status", "Col.Id": "Id", "Col.Actions": "Aktionen",
    "Origin": "Abflug", "Destination": "Ziel", "Aircraft": "Flugzeug",
    "DepartureTime": "Abflugzeit", "ArrivalTime": "Ankunftszeit", "Status": "Status",
    "Name": "Name", "City": "Stadt", "Country": "Land", "IATA": "IATA",
    "Latitude": "Breitengrad", "Longitude": "Längengrad", "Model": "Modell",
    "TakeOffEffort": "Startaufwand", "FuelConsumptionPerKm": "Kraftstoffverbrauch / km",
    "CruiseSpeedKmh": "Reisegeschwindigkeit (km/h)",
    "Label.DistanceKm": "Entfernung (km)", "Label.Fuel": "Kraftstoff", "Label.ArrivalTime": "Ankunftszeit",
    "Label.Locked": "Gesperrt", "Label.Calculated": "Berechnet",
    "FlightStatus.Scheduled": "Geplant", "FlightStatus.Departed": "Abgeflogen",
    "FlightStatus.Arrived": "Angekommen", "FlightStatus.Cancelled": "Storniert",
    "Error.CreateFlightFailed": "Flug konnte nicht erstellt werden. Flughäfen und Flugzeug prüfen.",
    "Error.UpdateFlightFailed": "Flug konnte nicht aktualisiert werden.",
    "Error.CreateAirportFailed": "Flughafen konnte nicht erstellt werden.",
    "Error.UpdateAirportFailed": "Flughafen konnte nicht aktualisiert werden.",
    "Error.DeleteAirportReferenced": "Löschen nicht möglich: Flughafen wird von Flügen referenziert.",
    "Error.CreateAircraftFailed": "Flugzeug konnte nicht erstellt werden.",
    "Error.UpdateAircraftFailed": "Flugzeug konnte nicht aktualisiert werden.",
    "Error.DeleteAircraftReferenced": "Löschen nicht möglich: Flugzeug wird von Flügen referenziert.",
    "Confirm.DeleteFlight": "Flug #{0} löschen?",
    "Confirm.DeleteAirport": "{0} löschen?",
    "Confirm.DeleteAircraft": "{0} löschen?",
    "Confirm.CannotUndo": "Diese Aktion kann nicht widerrufen werden.",
    "Empty.Flights.Title": "Noch keine Flüge",
    "Empty.Flights.Message": "Erstellen Sie Ihren ersten Flug, um den Betrieb zu verfolgen.",
    "Empty.Airports.Title": "Noch keine Flughäfen",
    "Empty.Airports.Message": "Fügen Sie einen Flughafen hinzu, um Ihr Netzwerk aufzubauen.",
    "Empty.Aircraft.Title": "Noch keine Flugzeuge",
    "Empty.Aircraft.Message": "Fügen Sie ein Flugzeug hinzu, um Flüge zuzuweisen.",
    "Alert.EditBlocked": "Dieser Flug kann nicht bearbeitet werden, da der Status {0} ist.",
    "Report.Summary": "Zusammenfassung aller Flüge mit berechneter Entfernung und Kraftstoff.",
    "Home.HeroTitle": "Flugbetriebszentrale",
    "Home.HeroSubtitle": "Verwalten Sie Flughäfen, Flotte und Flüge mit Echtzeit-Entfernungs- und Kraftstoffberechnungen.",
    "Home.Card.Airports": "Flughäfen", "Home.Card.Aircrafts": "Flotte", "Home.Card.Flights": "Flüge",
    "Home.Card.Report": "Berichte", "Home.Card.Simulation": "Live-Simulation",
    "Home.Card.AirportsDesc": "Flughafenregister und Koordinaten verwalten.",
    "Home.Card.AircraftsDesc": "Flugzeugleistungsprofile konfigurieren.",
    "Home.Card.FlightsDesc": "Flugbetrieb aircraftn und verfolgen.",
    "Home.Card.ReportDesc": "Betriebsübersichten und Kennzahlen anzeigen.",
    "Home.Card.SimulationDesc": "Beobachten Sie Flüge in Echtzeit über den Globus.",
    "Privacy.Intro": "Diese Richtlinie beschreibt, wie FlightOps Ihre Daten behandelt.",
    "Privacy.DataCollected": "Erhobene Daten",
    "Privacy.DataCollectedText": "FlightOps speichert von Ihnen eingegebene Betriebsdaten: Flughäfen, Flugzeuge und Flugpläne. Persönliche Benutzerkonten sind nicht erforderlich.",
    "Privacy.Purpose": "Zweck",
    "Privacy.PurposeText": "Daten werden ausschließlich für Flugmanagement- und Simulationsfunktionen in dieser Anwendung verwendet.",
    "Privacy.Contact": "Kontakt",
    "Privacy.ContactText": "Bei Datenschutzfragen wenden Sie sich an den FlightOps-Administrator.",
    "Privacy.Cookies": "Cookies",
    "Privacy.CookiesText": "Wir verwenden ein Kultur-Cookie, um Ihre Sprachauswahl zu speichern.",
    "Simulation.SelectRoute": "Route für Simulation auswählen",
    "Simulation.Start": "Starten",
    "Simulation.Distance": "Entfernung",
    "Simulation.Duration": "Geschätzte Dauer",
    "Simulation.Instructions": "Wählen Sie Abflug- und Zielflughafen, dann starten Sie die Simulation.",
    "Error.PageTitle": "Fehler.",
    "Error.Message": "Bei der Verarbeitung Ihrer Anfrage ist ein Fehler aufgetreten.",
    "Error.RequestId": "Anfrage-ID:",
    "Error.DevModeTitle": "Entwicklungsmodus",
    "Error.DevModeText1": "Im Development-Modus werden detailliertere Fehlerinformationen angezeigt.",
    "Error.DevModeText2": "Der Development-Modus sollte in produktiven Anwendungen nicht aktiviert sein.",
}

RESX_HEADER = """<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
"""


def write_resx(path: Path, keys: dict) -> None:
    lines = [RESX_HEADER]
    for k, v in sorted(keys.items()):
        v_esc = v.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
        lines.append(f'  <data name="{k}" xml:space="preserve">\n    <value>{v_esc}</value>\n  </data>\n')
    lines.append("</root>\n")
    path.write_text("".join(lines), encoding="utf-8")


base = Path(__file__).resolve().parent.parent / "Resources"
write_resx(base / "SharedResources.resx", keys_en)
write_resx(base / "SharedResources.pt-PT.resx", keys_pt)
write_resx(base / "SharedResources.de-DE.resx", keys_de)
print("done")
