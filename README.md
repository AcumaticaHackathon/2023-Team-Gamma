# 2023-Team-Gamma
This is the repository for Team Gamma's Hackathon project that was held on January 28th & 29th at the 2023 Summit at the Wynn Hotel in Las Vegas, Nevada.

This project allows a warehouse shipping dispatcher to group shipments together into a route, to be delivered on a company-owned vehicle.  The software then runs the shipment through an outside API called Route4me.io that will optimize the sequence of deliveries for those shipments in the route.  At the same time, the user can assign the route to a drivee (An Employee defined in Acumatica). The routing information is stored with each shipment. 

After the routing information is updated, the software will allow a driver to change the status of all of the shipments within this route to "Picked Up".  This will then cause a new status to be set on the shipemets to "Out for Delivery".   Later, the user can update each shipment with the actual time noted as the delivery time,
Dashboards were also created - to show how many stops are remaining to be delvered for a given driver, and also a map rendering that shows the driving path and stops along the way.  This map can be useed by the driver to assist with getting to each delivery location.
