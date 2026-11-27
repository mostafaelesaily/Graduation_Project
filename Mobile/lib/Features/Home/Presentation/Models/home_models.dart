import 'package:flutter/material.dart';

class UpcomingAppointment {
  final String doctorName;
  final String specialty;
  final String hospital;
  final String dayLabel;
  final String timeLabel;
  final bool isVideo;

  const UpcomingAppointment({
    required this.doctorName,
    required this.specialty,
    required this.hospital,
    required this.dayLabel,
    required this.timeLabel,
    this.isVideo = false,
  });
}

class CareService {
  final String label;
  final IconData icon;

  const CareService({required this.label, required this.icon});
}

/// Temporary static data until the Home endpoints are available in the API.
class HomeMockData {
  static const List<UpcomingAppointment> appointments = [
    UpcomingAppointment(
      doctorName: 'Dr. Sarah Jenkins, MD',
      specialty: 'Lead Cardiologist',
      hospital: 'Mass General',
      dayLabel: 'Tomorrow',
      timeLabel: '10:30 AM',
      isVideo: true,
    ),
    UpcomingAppointment(
      doctorName: 'Dr. Omar Hassan',
      specialty: 'Dermatologist',
      hospital: 'City Clinic',
      dayLabel: 'Friday',
      timeLabel: '1:00 PM',
    ),
  ];

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
