import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';
import 'package:medlink/Features/Appointments/Presentation/Widgets/appointment_card.dart';

/// What the patient chose to do before closing the sheet.
class AppointmentSheetResult {
  final bool cancel;
  final String notes;

  const AppointmentSheetResult.cancel() : cancel = true, notes = '';

  const AppointmentSheetResult.saveNotes(this.notes) : cancel = false;
}

/// Notes can be edited and the visit cancelled only while it is upcoming.
class AppointmentDetailsSheet extends StatefulWidget {
  final Appointment appointment;

  const AppointmentDetailsSheet({super.key, required this.appointment});

  @override
  State<AppointmentDetailsSheet> createState() =>
      _AppointmentDetailsSheetState();
}

class _AppointmentDetailsSheetState extends State<AppointmentDetailsSheet> {
  late final TextEditingController _notes;

  @override
  void initState() {
    super.initState();
    _notes = TextEditingController(text: widget.appointment.notes ?? '');
  }

  @override
  void dispose() {
    _notes.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final appointment = widget.appointment;

    return Padding(
      // Keeps the notes field above the keyboard.
      padding: EdgeInsets.only(bottom: MediaQuery.viewInsetsOf(context).bottom),
      child: SingleChildScrollView(
        padding: EdgeInsets.all(20.w),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          spacing: 12.h,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(appointment.title, style: TextStyles.authTitle),
                ),
                AppointmentStatusBadge(status: appointment.status),
              ],
            ),
            _buildRow(Icons.calendar_month, appointment.dateLabel),
            if (appointment.address.isNotEmpty)
              _buildRow(Icons.location_on_outlined, appointment.address),
            if (appointment.phoneNumber.isNotEmpty)
              _buildRow(Icons.phone_outlined, appointment.phoneNumber),
            if (appointment.isUpcoming) ..._buildActions(context),
            if (!appointment.isUpcoming &&
                (appointment.notes?.isNotEmpty ?? false))
              _buildRow(Icons.notes, appointment.notes!),
          ],
        ),
      ),
    );
  }

  List<Widget> _buildActions(BuildContext context) {
    return [
      TextField(
        controller: _notes,
        maxLength: 1000,
        minLines: 2,
        maxLines: 5,
        decoration: InputDecoration(
          labelText: 'Notes',
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(16.r)),
        ),
      ),
      AuthButton(
        onPressed: () => Navigator.pop(
          context,
          AppointmentSheetResult.saveNotes(_notes.text.trim()),
        ),
        text: 'Save Notes',
      ),
      TextButton(
        onPressed: () async {
          final confirmed = await confirmCancelAppointment(context);
          if (confirmed && context.mounted) {
            Navigator.pop(context, const AppointmentSheetResult.cancel());
          }
        },
        style: TextButton.styleFrom(foregroundColor: Colors.red),
        child: const Text('Cancel Appointment'),
      ),
    ];
  }

  Widget _buildRow(IconData icon, String text) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      spacing: 10.w,
      children: [
        Icon(icon, color: ColorsManager.primaryColor, size: 20.sp),
        Expanded(
          child: Text(
            text,
            style: TextStyle(color: ColorsManager.textDark, fontSize: 14.sp),
          ),
        ),
      ],
    );
  }
}

Future<bool> confirmCancelAppointment(BuildContext context) async {
  final confirmed = await showDialog<bool>(
    context: context,
    builder: (context) => AlertDialog(
      title: const Text('Cancel appointment?'),
      content: const Text('This cannot be undone.'),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context, false),
          child: const Text('Keep'),
        ),
        TextButton(
          onPressed: () => Navigator.pop(context, true),
          style: TextButton.styleFrom(foregroundColor: Colors.red),
          child: const Text('Cancel it'),
        ),
      ],
    ),
  );

  return confirmed == true;
}
