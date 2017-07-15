/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#include <QObject>
#include <QString>

/*!
 * \brief The TimeConverter class converts seconds to a human readable string.
 *
 * Since this is used from QML with property bindings, the conversion can't
 * just be done with one static function.
 *
 * TODO: Make this work with just one static function. This class is overkill.
 */
class TimeConverter : public QObject {
  Q_OBJECT

  Q_PROPERTY(int seconds READ seconds WRITE setSeconds NOTIFY secondsChanged)
  Q_PROPERTY(QString timestring READ timestring NOTIFY timestringChanged)

 public:
  TimeConverter(QObject *parent = 0);

  void setSeconds(int sec);
  int seconds();

  QString timestring();

  QString toString();

  static const int minInSec = 60;
  static const int hourInSec = 60 * minInSec;
  static const int dayInSec = 24 * hourInSec;

  QString numToString(int num);

  void clear();

 signals:
  void secondsChanged();
  void timestringChanged();

 private:
  int totalSec_;
  int sec_;
  int mins_;
  int hours_;
  int days_;
};
