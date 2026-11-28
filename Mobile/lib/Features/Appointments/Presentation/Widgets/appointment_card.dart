import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';

class AppointmentCard extends StatelessWidget {
  final Appointment appointment;

  final VoidCallback? onViewDetails;
  final VoidCallback? onDirectionsTap;

  /// The three below are null once the appointment is no longer upcoming.
  final VoidCallback? onAddToCalendar;
  final VoidCallback? onEditNotes;
  final VoidCallback? onCancel;

  const AppointmentCard({
    super.key,
    required this.appointment,
    this.onViewDetails,
    this.onDirectionsTap,
    this.onAddToCalendar,
    this.onEditNotes,
    this.onCancel,
  });

  static const primaryColor = Color(0xFF3182F6);
  static const textColor = Color(0xFF173554);

  @override
  Widget build(BuildContext context) {
    final notes = appointment.notes;

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(26),
        border: Border.all(color: const Color(0xFFDCEAFF)),
        boxShadow: [
          BoxShadow(
            color: primaryColor.withValues(alpha: 0.05),
            blurRadius: 20,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        spacing: 20.w,
        mainAxisSize: MainAxisSize.min,
        children: [
          // Provider information
          _DoctorInfo(name: appointment.title, status: appointment.status),

          // Appointment information
          _AppointmentInfo(
            date: appointment.dateLabel,
            location: appointment.address,
            onDirectionsTap: onDirectionsTap,
          ),

          if (notes != null && notes.isNotEmpty)
            _AppointmentNotes(notes: notes),

          // Actions
          Row(
            spacing: 10.w,
            children: [
              Expanded(
                child: AuthButton(
                  onPressed: onViewDetails,
                  text: 'View Details',
                ),
              ),
              if (onAddToCalendar != null)
                _ActionIcon(
                  icon: Icons.event_available_outlined,
                  tooltip: 'Add to calendar',
                  onTap: onAddToCalendar!,
                ),
              if (onEditNotes != null && onCancel != null)
                _MoreMenu(onEditNotes: onEditNotes!, onCancel: onCancel!),
            ],
          ),
        ],
      ),
    );
  }
}

class _DoctorInfo extends StatelessWidget {
  final String name;
  final AppointmentStatus status;

  const _DoctorInfo({required this.name, required this.status});

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        Container(
          width: 48,
          height: 48,
          decoration: BoxDecoration(
            color: ColorsManager.softBlue,
            borderRadius: BorderRadius.circular(18),
          ),
          child: const Icon(Icons.person, color: AppointmentCard.primaryColor),
        ),

        const SizedBox(width: 14),

        Expanded(
          child: Text(
            name,
            style: const TextStyle(
              fontSize: 17,
              fontWeight: FontWeight.w700,
              color: AppointmentCard.textColor,
            ),
          ),
        ),

        const SizedBox(width: 8),

        AppointmentStatusBadge(status: status),
      ],
    );
  }
}

class AppointmentStatusBadge extends StatelessWidget {
  final AppointmentStatus status;

  const AppointmentStatusBadge({super.key, required this.status});

  @override
  Widget build(BuildContext context) {
    final (label, icon, color) = switch (status) {
      AppointmentStatus.pending => (
        'Pending',
        Icons.schedule,
        const Color(0xFFB45309),
      ),
      AppointmentStatus.confirmed => (
        'Confirmed',
        Icons.check_circle_outline,
        const Color(0xFF059669),
      ),
      AppointmentStatus.completed => (
        'Completed',
        Icons.task_alt,
        AppointmentCard.primaryColor,
      ),
      AppointmentStatus.cancelled => (
        'Cancelled',
        Icons.cancel_outlined,
        const Color(0xFFDC2626),
      ),
      AppointmentStatus.rejected => (
        'Rejected',
        Icons.block,
        const Color(0xFFDC2626),
      ),
      AppointmentStatus.expired => (
        'Expired',
        Icons.timer_off_outlined,
        ColorsManager.fadedText,
      ),
    };

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(30),
        border: Border.all(color: color.withValues(alpha: 0.4)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 11, color: color),
          const SizedBox(width: 6),
          Text(
            label,
            style: TextStyle(
              color: color,
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

          if (location.isNotEmpty) ...[
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
                    'Directions ↱',
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

class _ActionIcon extends StatelessWidget {
  final IconData icon;
  final String tooltip;
  final VoidCallback onTap;

  const _ActionIcon({
    required this.icon,
    required this.tooltip,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Tooltip(
      message: tooltip,
      child: SizedBox(
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
      ),
    );
  }
}

class _MoreMenu extends StatelessWidget {
  final VoidCallback onEditNotes;
  final VoidCallback onCancel;

  const _MoreMenu({required this.onEditNotes, required this.onCancel});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 44.w,
      height: 44.w,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(15),
        border: Border.all(color: const Color(0xFFDCEAFF)),
      ),
      child: PopupMenuButton<VoidCallback>(
        padding: EdgeInsets.zero,
        color: Colors.white,
        icon: const Icon(Icons.more_vert, color: Color(0xFF66809F)),
        onSelected: (action) => action(),
        itemBuilder: (context) => [
          PopupMenuItem(value: onEditNotes, child: const Text('Edit notes')),
          PopupMenuItem(
            value: onCancel,
            child: const Text(
              'Cancel appointment',
              style: TextStyle(color: Colors.red),
            ),
          ),
        ],
      ),
    );
  }
}
