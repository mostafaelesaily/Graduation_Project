import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Core/Style/text_styles.dart';
import 'package:medlink/Core/Dialogs/error_dialog.dart';
import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_cubit.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_state.dart';
import 'package:medlink/Features/Appointments/Presentation/Widgets/appointment_card.dart';
import 'package:medlink/Features/Appointments/Presentation/Widgets/appointment_details_sheet.dart';
import 'package:medlink/Features/Appointments/Presentation/Widgets/follow_up_card.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_header.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';
import 'package:url_launcher/url_launcher.dart';

class AppointmentsScreen extends StatelessWidget {
  const new({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Container(
          width: double.infinity,
          padding: EdgeInsets.fromLTRB(20.w, 8.h, 20.w, 10.h),
          decoration: const BoxDecoration(
            color: Colors.white,
            border: Border(bottom: BorderSide(color: ColorsManager.cardBorder)),
          ),
          child: const HomeHeader(title: 'Appointments'),
        ),
        Expanded(
          child: Padding(
            padding: EdgeInsets.fromLTRB(20.w, 16.h, 20.w, 0),
            child: Column(
              spacing: 16.h,
              children: [_Title(), _Filters(), _Appointments()],
            ),
          ),
        ),
      ],
    );
  }
}

class _Title extends StatelessWidget {
  const new({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: .stretch,
      children: [
        Text('Your Care Schedule', style: TextStyles.authTitle),
        Text(
          'Track, prepare for, and manage your medical consultations',
          style: TextStyles.fadedText,
        ),
      ],
    );
  }
}

class _Filters extends StatelessWidget {
  const new({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocSelector<
      AppointmentsCubit,
      AppointmentsState,
      AppointmentFilter
    >(
      selector: (state) => state.filter,
      builder: (context, chosenFilter) {
        return Container(
          padding: EdgeInsets.all(4),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16),
            border: BoxBorder.all(color: ColorsManager.cardBorder),
          ),
          child: Row(
            spacing: 4,
            mainAxisAlignment: .spaceAround,
            children: [
              _FiltersButton(
                title: 'Upcoming',
                isChosen: chosenFilter == AppointmentFilter.upcoming,
                onPressed: () {
                  context.read<AppointmentsCubit>().changeFilter(
                    AppointmentFilter.upcoming,
                  );
                },
              ),
              _FiltersButton(
                title: 'Past Visits',
                isChosen: chosenFilter == AppointmentFilter.past,
                onPressed: () {
                  context.read<AppointmentsCubit>().changeFilter(
                    AppointmentFilter.past,
                  );
                },
              ),
              _FiltersButton(
                title: 'Cancelled',
                isChosen: chosenFilter == AppointmentFilter.cancelled,
                onPressed: () {
                  context.read<AppointmentsCubit>().changeFilter(
                    AppointmentFilter.cancelled,
                  );
                },
              ),
            ],
          ),
        );
      },
    );
  }
}

class _Appointments extends StatelessWidget {
  const new();

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: BlocBuilder<AppointmentsCubit, AppointmentsState>(
        builder: (context, state) {
          final cubit = context.read<AppointmentsCubit>();

          if (state.status == AppointmentsStatus.loading) {
            return const Center(child: CircularProgressIndicator());
          }

          final appointments = state.byFilter(state.filter);
          final lastVisit = state.filter == AppointmentFilter.upcoming
              ? _lastVisit(state)
              : null;
          // An empty list still takes one row, for the message.
          final rows = appointments.isEmpty ? 1 : appointments.length;

          return RefreshIndicator(
            onRefresh: cubit.load,
            child: ListView.separated(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: EdgeInsets.only(bottom: 24.h),
              itemCount: rows + (lastVisit == null ? 0 : 1),
              separatorBuilder: (context, index) => SizedBox(height: 16.h),
              itemBuilder: (context, index) {
                if (index == rows) {
                  return FollowUpCard(
                    providerName: lastVisit!.providerName,
                    onRebook: () => _rebook(context, lastVisit),
                  );
                }

                if (appointments.isEmpty) {
                  return _Message(
                    hasError: state.status == AppointmentsStatus.failure,
                    onRetry: cubit.load,
                  );
                }

                final appointment = appointments[index];

                final isUpcoming = appointment.isUpcoming;

                return AppointmentCard(
                  appointment: appointment,
                  onViewDetails: () => _showDetails(context, appointment),
                  onDirectionsTap: appointment.address.isEmpty
                      ? null
                      : () => _openMap(appointment.address),
                  onAddToCalendar: isUpcoming
                      ? () => _addToCalendar(appointment)
                      : null,
                  // The notes field is in the details sheet.
                  onEditNotes: isUpcoming
                      ? () => _showDetails(context, appointment)
                      : null,
                  onCancel: isUpcoming
                      ? () => _cancel(context, appointment)
                      : null,
                );
              },
            ),
          );
        },
      ),
    );
  }

  Future<void> _showDetails(
    BuildContext context,
    Appointment appointment,
  ) async {
    final cubit = context.read<AppointmentsCubit>();

    final result = await showModalBottomSheet<AppointmentSheetResult>(
      context: context,
      isScrollControlled: true,
      useSafeArea: true,
      backgroundColor: Colors.white,
      builder: (context) => AppointmentDetailsSheet(appointment: appointment),
    );
    if (result == null) return;

    final error = result.cancel
        ? await cubit.cancel(appointment.id)
        : await cubit.updateNotes(appointment.id, result.notes);
    if (!context.mounted) return;

    _showResult(
      context,
      error,
      result.cancel ? 'Appointment cancelled.' : 'Notes saved.',
    );
  }

  Future<void> _cancel(BuildContext context, Appointment appointment) async {
    final cubit = context.read<AppointmentsCubit>();
    if (!await confirmCancelAppointment(context)) return;

    final error = await cubit.cancel(appointment.id);
    if (!context.mounted) return;

    _showResult(context, error, 'Appointment cancelled.');
  }

  void _showResult(BuildContext context, String? error, String success) {
    if (error != null) {
      showDialog(
        context: context,
        builder: (context) => ErrorDialog(error: error),
      );
      return;
    }

    ScaffoldMessenger.of(context)
        .showSnackBar(SnackBar(content: Text(success)));
  }

  /// The latest visit that took place and whose provider is known.
  Appointment? _lastVisit(AppointmentsState state) {
    for (final appointment in state.byFilter(AppointmentFilter.past)) {
      final visited =
          appointment.status == AppointmentStatus.completed ||
          appointment.status == AppointmentStatus.confirmed;

      if (visited && appointment.providerName.isNotEmpty) return appointment;
    }

    return null;
  }

  Future<void> _rebook(BuildContext context, Appointment appointment) async {
    final cubit = context.read<AppointmentsCubit>();

    await context.push(
      '/provider',
      extra: ProviderProfile(
        id: appointment.providerId,
        serviceName: appointment.providerName,
        address: appointment.address,
        phoneNumber: appointment.phoneNumber,
      ),
    );

    if (!cubit.isClosed) await cubit.load();
  }

  /// Opens a Google Calendar event, assuming a visit takes half an hour.
  Future<void> _addToCalendar(Appointment appointment) async {
    String format(DateTime date) {
      String two(int value) => '$value'.padLeft(2, '0');

      return '${date.year}${two(date.month)}${two(date.day)}'
          'T${two(date.hour)}${two(date.minute)}00';
    }

    final end = appointment.date.add(const Duration(minutes: 30));

    await launchUrl(
      Uri.https('calendar.google.com', '/calendar/render', {
        'action': 'TEMPLATE',
        'text': 'Appointment with ${appointment.title}',
        'dates': '${format(appointment.date)}/${format(end)}',
        'location': appointment.address,
      }),
      mode: LaunchMode.externalApplication,
    );
  }

  Future<void> _openMap(String address) async {
    await launchUrl(
      Uri.https('www.google.com', '/maps/search/', {
        'api': '1',
        'query': address,
      }),
      mode: LaunchMode.externalApplication,
    );
  }
}

class _Message extends StatelessWidget {
  final bool hasError;
  final VoidCallback onRetry;

  const _Message({required this.hasError, required this.onRetry});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.only(top: 80.h),
      child: Column(
        children: [
          Text(
            hasError
                ? 'Could not load your appointments.'
                : 'No appointments here yet.',
            style: TextStyles.fadedText,
          ),
          if (hasError)
            TextButton(onPressed: onRetry, child: const Text('Retry')),
        ],
      ),
    );
  }
}

class _FiltersButton extends StatelessWidget {
  final bool isChosen;
  final String title;
  final void Function()? onPressed;
  const new({
    this.isChosen = false,
    required this.title,
    required this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: TextButton(
        onPressed: onPressed,
        style: TextButton.styleFrom(
          backgroundColor: isChosen ? Color(0xffEAF4FF) : null,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadiusGeometry.circular(12),
          ),
          maximumSize: Size(double.infinity, 37.5),
          minimumSize: Size(double.infinity, 37.5),
          padding: EdgeInsets.zero,
          tapTargetSize: .shrinkWrap,
          visualDensity: .standard,
        ),

        child: Text(
          title,
          style: TextStyle(
            color: !isChosen ? Color(0xff6B8299) : ColorsManager.primaryColor,
            fontSize: 13,
            fontWeight: .w600,
          ),
        ),
      ),
    );
  }
}
