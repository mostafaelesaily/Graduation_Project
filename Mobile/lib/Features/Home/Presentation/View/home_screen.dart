import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_cubit.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_state.dart';
import 'package:medlink/Features/Appointments/Presentation/View/appointments_screen.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_bottom_nav.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_tab.dart';

class HomeScreen extends StatefulWidget {
  /// TODO: replace with the logged-in user's name once the profile endpoint
  /// is wired in.
  final String userName;

  const HomeScreen({super.key, this.userName = 'Sarah'});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  static const _appointmentsIndex = 2;

  int _currentIndex = 0;

  void _openTab(int index) {
    if (index == _appointmentsIndex) {
      context.read<AppointmentsCubit>().load();
    }

    setState(() => _currentIndex = index);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: ColorsManager.primaryBackgroundColor,
      body: SafeArea(
        bottom: false,
        child: IndexedStack(
          index: _currentIndex,
          children: [
            HomeTab(
              userName: widget.userName,
              onViewAllAppointments: () {
                context.read<AppointmentsCubit>().changeFilter(
                  AppointmentFilter.upcoming,
                );
                _openTab(_appointmentsIndex);
              },
            ),
            const _ComingSoonTab(title: 'Search'),
            const AppointmentsScreen(),
            const _ComingSoonTab(title: 'Records'),
            const _ComingSoonTab(title: 'Profile'),
          ],
        ),
      ),
      bottomNavigationBar:
          BlocSelector<AppointmentsCubit, AppointmentsState, int>(
            selector: (state) =>
                state.byFilter(AppointmentFilter.upcoming).length,
            builder: (context, upcomingCount) {
              return HomeBottomNav(
                currentIndex: _currentIndex,
                appointmentsBadge: upcomingCount,
                onTap: _openTab,
              );
            },
          ),
    );
  }
}

class _ComingSoonTab extends StatelessWidget {
  final String title;

  const _ComingSoonTab({required this.title});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Text(
        '$title - coming soon',
        style: const TextStyle(color: ColorsManager.fadedText, fontSize: 16),
      ),
    );
  }
}
