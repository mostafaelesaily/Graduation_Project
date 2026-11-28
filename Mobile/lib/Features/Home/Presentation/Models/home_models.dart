import 'package:flutter/material.dart';

class CareService {
  final String label;
  final IconData icon;

  const CareService({required this.label, required this.icon});
}

/// Temporary static data until the Home endpoints are available in the API.
class HomeMockData {
  static const List<CareService> services = [
    CareService(
      label: 'Doctors & Specialists',
      icon: Icons.medical_services_outlined,
    ),
    CareService(label: 'Clinics', icon: Icons.add_box_outlined),
    CareService(label: 'Medical Labs', icon: Icons.biotech_outlined),
    CareService(
      label: 'Radiology & MRI',
      icon: Icons.medical_information_outlined,
    ),
    CareService(label: 'Hospitals', icon: Icons.local_hospital_outlined),
    CareService(label: 'Nearby', icon: Icons.location_on_outlined),
  ];
}
