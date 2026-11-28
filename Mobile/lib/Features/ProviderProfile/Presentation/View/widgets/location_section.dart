import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:latlong2/latlong.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/ProviderProfile/Data/datasources/geocoding_data_source.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/widgets/section_card.dart';
import 'package:url_launcher/url_launcher.dart';

class LocationSection extends StatefulWidget {
  final String address;

  const LocationSection({super.key, required this.address});

  @override
  State<LocationSection> createState() => _LocationSectionState();
}

class _LocationSectionState extends State<LocationSection> {
  late final Future<LatLng?> _location;

  @override
  void initState() {
    super.initState();
    _location = widget.address.isEmpty
        ? Future.value(null)
        : GeocodingDataSource().findAddress(widget.address);
  }

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Practice Location',
      child: FutureBuilder<LatLng?>(
        future: _location,
        builder: (context, snapshot) {
          final point = snapshot.data;

          return Column(
            children: [
              if (point != null) ...[_buildMap(point), SizedBox(height: 10.h)],
              _buildAddressRow(),
            ],
          );
        },
      ),
    );
  }

  Widget _buildMap(LatLng point) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(16.r),
      child: SizedBox(
        height: 160.h,
        child: Stack(
          children: [
            FlutterMap(
              options: MapOptions(
                initialCenter: point,
                initialZoom: 14,
                interactionOptions: const InteractionOptions(
                  flags: InteractiveFlag.none,
                ),
                onTap: (_, _) => _openMap(),
              ),
              children: [
                TileLayer(
                  urlTemplate:
                      'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                  userAgentPackageName: 'com.example.medlink',
                ),
                MarkerLayer(
                  markers: [
                    Marker(
                      point: point,
                      child: const Icon(
                        Icons.location_on,
                        color: ColorsManager.primaryColor,
                        size: 32,
                      ),
                    ),
                  ],
                ),
              ],
            ),
            Positioned(
              right: 4,
              bottom: 2,
              child: Text(
                '© OpenStreetMap contributors',
                style: TextStyle(fontSize: 9.sp, color: Colors.black87),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAddressRow() {
    return Row(
      children: [
        Icon(
          Icons.location_on_outlined,
          color: ColorsManager.primaryColor,
          size: 20.sp,
        ),
        SizedBox(width: 8.w),
        Expanded(
          child: Text(
            widget.address.isEmpty ? 'No address listed.' : widget.address,
            style: TextStyle(color: ColorsManager.textDark, fontSize: 14.sp),
          ),
        ),
        if (widget.address.isNotEmpty)
          TextButton(
            onPressed: _openMap,
            style: TextButton.styleFrom(
              foregroundColor: ColorsManager.primaryColor,
            ),
            child: const Text('Map'),
          ),
      ],
    );
  }

  Future<void> _openMap() async {
    await launchUrl(
      Uri.https('www.google.com', '/maps/search/', {
        'api': '1',
        'query': widget.address,
      }),
      mode: LaunchMode.externalApplication,
    );
  }
}
