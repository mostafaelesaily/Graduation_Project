import 'package:flutter/material.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

class _Tones {
  static const Color blue = ColorsManager.primaryColor;
  static const Color green = Color(0xff16A34A);
  static const Color teal = Color(0xff06B6D4);
  static const Color grey = Color(0xff475569);
}

class OnboardingTileData {
  final IconData icon;
  final String title;
  final String subtitle;
  final String badge;
  final Color iconColor;
  final Color badgeColor;

  const OnboardingTileData({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.badge,
    required this.iconColor,
    required this.badgeColor,
  });
}

class OnboardingItem {
  final String image;
  final String title;
  final String description;
  final List<OnboardingTileData> tiles;

  const OnboardingItem({
    required this.image,
    required this.title,
    required this.description,
    required this.tiles,
  });
}

/// Content of the 3 onboarding pages (copied from the Figma design).
const List<OnboardingItem> onboardingItems = [
  OnboardingItem(
    image: 'assets/images/onboarding_1.png',
    title: 'Find the Right Care,\nRight When You Need It',
    description:
        'Describe symptoms in plain language to our AI assistant, find '
        'verified in-network specialists, and book appointments in seconds.',
    tiles: [
      OnboardingTileData(
        icon: Icons.auto_awesome,
        title: 'Smart AI Medical Triage',
        subtitle: 'Fast, compassionate symptom pre-check',
        badge: 'AI Care',
        iconColor: _Tones.blue,
        badgeColor: _Tones.blue,
      ),
      OnboardingTileData(
        icon: Icons.check_circle,
        title: 'Verified Doctors & Diagnostics',
        subtitle: 'Licensed practitioners & accredited labs',
        badge: 'Verified',
        iconColor: _Tones.green,
        badgeColor: _Tones.green,
      ),
      OnboardingTileData(
        icon: Icons.shield,
        title: 'Unified Health History & Records',
        subtitle: 'Prescriptions & visit notes in one portal',
        badge: 'Encrypted',
        iconColor: _Tones.teal,
        badgeColor: _Tones.grey,
      ),
    ],
  ),
  OnboardingItem(
    image: 'assets/images/onboarding_2.png',
    title: 'Verified Specialists &\nEasy Booking',
    description:
        'Find board-certified, in-network doctors tailored to your needs. '
        'Compare real patient reviews and book seamless in-clinic '
        'consultations.',
    tiles: [
      OnboardingTileData(
        icon: Icons.check_circle,
        title: 'Verified Doctors & Diagnostics',
        subtitle: 'Licensed practitioners & accredited labs',
        badge: 'Verified',
        iconColor: _Tones.green,
        badgeColor: _Tones.green,
      ),
      OnboardingTileData(
        icon: Icons.event_available_outlined,
        title: 'Instant Schedule Confirmation',
        subtitle: 'Book in seconds with live specialist availability',
        badge: 'Real-Time',
        iconColor: _Tones.blue,
        badgeColor: _Tones.blue,
      ),
      OnboardingTileData(
        icon: Icons.shield,
        title: 'In-Network Insurance Match',
        subtitle: 'Transparent co-pays & coverage checks',
        badge: '100% Covered',
        iconColor: _Tones.teal,
        badgeColor: _Tones.grey,
      ),
    ],
  ),
  OnboardingItem(
    image: 'assets/images/onboarding_3.png',
    title: 'Your Complete Health History,\nUnified & Secure',
    description:
        'Keep your prescriptions, lab diagnostic reports, and clinician '
        'visit notes encrypted in one central vault accessible anywhere.',
    tiles: [
      OnboardingTileData(
        icon: Icons.folder,
        title: 'Unified Health History & Records',
        subtitle: 'Prescriptions & visit notes in one portal',
        badge: 'Encrypted',
        iconColor: _Tones.blue,
        badgeColor: _Tones.blue,
      ),
      OnboardingTileData(
        icon: Icons.add_box,
        title: 'Automated Prescription Sync',
        subtitle: 'Pharmacy orders & automatic refill tracking',
        badge: 'Auto-Refill',
        iconColor: _Tones.green,
        badgeColor: _Tones.green,
      ),
      OnboardingTileData(
        icon: Icons.monitor_heart_outlined,
        title: 'Comprehensive Lab & Diagnostic Reports',
        subtitle: 'Instant delivery of bloodwork, imaging & scan results',
        badge: 'Instant Sync',
        iconColor: _Tones.teal,
        badgeColor: _Tones.grey,
      ),
    ],
  ),
];
