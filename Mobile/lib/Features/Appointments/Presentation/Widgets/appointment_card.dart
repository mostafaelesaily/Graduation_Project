import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Style/colors_manager.dart';

class AppointmentCard extends StatelessWidget {
  final String doctorName;
  final String specialty;
  final String date;
  final String location;
  final String imageUrl;
  final String? notes;

  final VoidCallback? onViewDetails;
  final VoidCallback? onCalendarTap;
  final VoidCallback? onMoreTap;
  final VoidCallback? onDirectionsTap;

  const AppointmentCard({
    super.key,
    required this.doctorName,
    required this.specialty,
    required this.date,
    required this.location,
    required this.imageUrl,
    this.notes,
    this.onViewDetails,
    this.onCalendarTap,
    this.onMoreTap,
    this.onDirectionsTap,
  });

  static const primaryColor = Color(0xFF3182F6);
  static const textColor = Color(0xFF173554);

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(26),
        border: Border.all(color: const Color(0xFFDCEAFF)),
      ),
      child: Column(
        spacing: 20.w,
        mainAxisSize: MainAxisSize.min,
        children: [
          // Doctor information
          _DoctorInfo(
            name: doctorName,
            specialty: specialty,
            imageUrl: imageUrl,
          ),

          // Appointment information
          _AppointmentInfo(
            date: date,
            location: location,
            onDirectionsTap: onDirectionsTap,
          ),

          if (notes == null) ...[
            _AppointmentNotes(
              notes: 'Do not eat or drink anything other than water for 8 hours prior to blood draw.',
            ),
          ],

          // Actions
          _AppointmentActions(
            onViewDetails: onViewDetails,
            onCalendarTap: onCalendarTap,
            onMoreTap: onMoreTap,
          ),
        ],
      ),
    );
  }
}

class _DoctorInfo extends StatelessWidget {
  final String name;
  final String specialty;
  final String imageUrl;

  const _DoctorInfo({
    required this.name,
    required this.specialty,
    required this.imageUrl,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        ClipRRect(
          borderRadius: BorderRadius.circular(18),
          child: Image.network(
            imageUrl,
            width: 48,
            height: 48,
            fit: BoxFit.cover,
            errorBuilder: (_, __, ___) => const SizedBox(
              width: 48,
              height: 48,
              child: Icon(Icons.person, size: 48),
            ),
          ),
        ),

        const SizedBox(width: 14),

        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                name,
                style: const TextStyle(
                  fontSize: 17,
                  fontWeight: FontWeight.w700,
                  color: AppointmentCard.textColor,
                ),
              ),
              const SizedBox(height: 3),
              Text(
                specialty,
                style: const TextStyle(
                  fontSize: 13,
                  fontWeight: .w600,
                  color: AppointmentCard.primaryColor,
                ),
              ),
            ],
          ),
        ),

        const SizedBox(width: 8),

        const _StatusBadge(),
      ],
    );
  }
}

class _StatusBadge extends StatelessWidget {
  const _StatusBadge();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 4),
      decoration: BoxDecoration(
        color: const Color(0xFFECFDF5),
        borderRadius: BorderRadius.circular(30),
        border: Border.all(color: const Color(0xFF86EFAC)),
      ),
      child: const Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.check_circle_outline, size: 11, color: Color(0xFF059669)),
          SizedBox(width: 6),
          Text(
            'Confirmed',
            style: TextStyle(
              color: Color(0xFF059669),
              fontSize: 11,
              fontWeight: FontWeight.w700,
            ),
          ),
        ],
      ),
    );
  }
}

class _AppointmentInfo extends StatelessWidget {
  final String date;
  final String location;
  final VoidCallback? onDirectionsTap;

  const _AppointmentInfo({
    required this.date,
    required this.location,
    this.onDirectionsTap,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFFF3F8FF),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFFE1EEFF)),
      ),
      child: Column(
        children: [
          Row(
            children: [
              const Icon(
                Icons.calendar_month,
                color: AppointmentCard.primaryColor,
                size: 23,
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  date,
                  style: const TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.w700,
                    color: AppointmentCard.textColor,
                  ),
                ),
              ),
            ],
          ),

          const SizedBox(height: 12),

          Row(
            children: [
              const Icon(
                Icons.location_on_outlined,
                color: Color(0xFF8CA3BB),
                size: 15,
              ),
              const SizedBox(width: 8),

              Expanded(
                child: Text(
                  location,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: Color(0xFF66809F),
                    fontSize: 13,
                    fontWeight: .w500,
                  ),
                ),
              ),

              TextButton(
                onPressed: onDirectionsTap,
                style: TextButton.styleFrom(
                  padding: const EdgeInsets.symmetric(horizontal: 4),
                  minimumSize: Size.zero,
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
                child: const Text(
                  'Directions',
                  style: TextStyle(
                    fontSize: 12,
                    fontWeight: .w700,
                    color: ColorsManager.primaryColor,
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _AppointmentNotes extends StatelessWidget {
  final String notes;

  const _AppointmentNotes({required this.notes});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xffFFFBEB),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: const Color(0xFFFDE68A).withValues(alpha: 0.7),
        ),
      ),
      child: Column(
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  notes,
                  maxLines: 20,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: Color(0xff78350F).withValues(alpha: 0.8),
                    fontSize: 12,
                    fontWeight: .w500,
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _AppointmentActions extends StatelessWidget {
  final VoidCallback? onViewDetails;
  final VoidCallback? onCalendarTap;
  final VoidCallback? onMoreTap;

  const _AppointmentActions({
    this.onViewDetails,
    this.onCalendarTap,
    this.onMoreTap,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      spacing: 10.w,
      children: [
        Expanded(
          child: AuthButton(onPressed: onViewDetails, text: 'View Details'),
        ),

        _ActionIcon(icon: Icons.event_available_outlined, onTap: onCalendarTap),

        _ActionIcon(icon: Icons.more_vert, onTap: onMoreTap),
      ],
    );
  }
}

class _ActionIcon extends StatelessWidget {
  final IconData icon;
  final VoidCallback? onTap;

  const _ActionIcon({required this.icon, this.onTap});

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 44.w,
      height: 44.w,
      child: OutlinedButton(
        onPressed: onTap,
        style: OutlinedButton.styleFrom(
          foregroundColor: const Color(0xFF66809F),
          padding: EdgeInsets.zero,
          side: const BorderSide(color: Color(0xFFDCEAFF)),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(15),
          ),
        ),
        child: Icon(icon, size: 23),
      ),
    );
  }
}
