# DistributedTracing on .Net
This sample project implemented by **.Net6** and shows how can collects logs (**OpenTelemetry**) in different projects and shows them in one place (**Jeager**)

I used these libraries in this sample:
- OpenTelemetry
- Entity Framework
- RabbitMQ
- MassTranist
- Redis
- Jeager

Before run these projects execute this `docker-compose up` in cmd.

To see the logs in **Jeager** go to `http://localhost:16686`

Also in visual studio change start up setting to **Multiple Startup** and set start mode for under projects:
- OrderApi
- InventoryApi
